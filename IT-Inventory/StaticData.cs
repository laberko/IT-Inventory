using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory
{
    public static class StaticData
    {
        public static void RefreshUsers()
        {
            using (var db = new InventoryModel())
            {
                //foreach (var deptokill in db.Departments.Where(d => d.Office == null).AsEnumerable())
                //{
                //    foreach (var user in db.Persons.Where(p => p.Dep.Id == deptokill.Id).AsEnumerable())
                //    {
                //        user.Dep = null;
                //        db.Entry(user).State = EntityState.Modified;
                //    }
                //    db.Departments.Remove(deptokill);
                //}
                //db.SaveChanges();

                try
                {
                    var adUsers = new List<Person>();
                    var newUsers = new List<string>();
                    var log = new StringBuilder();
                    using (var context = new PrincipalContext(ContextType.Domain, "rivs.org"))
                    using (var group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, "Пользователи домена"))
                    {
                        if (@group != null)
                        {
                            adUsers = new List<Person>();
                            foreach (var p in @group.GetMembers(false))
                            {
                                var entry = new DirectoryEntry("LDAP://DC=rivs,DC=org");
                                var mySearcher = new DirectorySearcher(entry)
                                {
                                    Filter = "(&(objectClass=person) (samaccountname=" + p.SamAccountName + "))"
                                };
                                var resultEntry = mySearcher.FindOne().GetDirectoryEntry();
                                if (resultEntry.Properties.Contains("DisplayName") &&
                                    !resultEntry.Properties.Contains("st") &&
                                    resultEntry.Properties.Contains("department") &&
                                    resultEntry.Properties.Contains("company"))
                                {
                                    var depName = resultEntry.Properties["company"].Value.ToString();
                                    var dep = db.Departments.FirstOrDefault(d => d.Name == depName);
                                    if (dep == null)
                                    {
                                        var newDep = new Department
                                        {
                                            Name = depName
                                        };
                                        db.Departments.Add(newDep);
                                        dep = newDep;
                                    }
                                    adUsers.Add(new Person
                                    {
                                        FullName = resultEntry.Properties["DisplayName"].Value.ToString(),
                                        AccountName = resultEntry.Properties["sAMAccountName"].Value.ToString(),
                                        Dep = dep,
                                        Email = resultEntry.Properties.Contains("mail") 
                                            ? resultEntry.Properties["mail"].Value.ToString() 
                                            : string.Empty,
                                        PhoneNumber = resultEntry.Properties.Contains("telephoneNumber") 
                                            ? resultEntry.Properties["telephoneNumber"].Value.ToString() 
                                            : string.Empty
                                        //IsItUser = resultEntry.Properties["department"].Value.ToString() == "Департамент информационных технологий"
                                    });
                                }
                            }
                        }
                    }
                    //add non-existing users to db or replace changed fields
                    foreach (var user in adUsers)
                    {
                        //find ad user in db
                        var existingPerson = db.Persons.FirstOrDefault(p => p.AccountName == user.AccountName);
                        var modified = false;
                        if (existingPerson == null)
                        {
                            db.Persons.Add(user);
                            newUsers.Add(user.FullName);
                        }
                        //full name changed
                        else if (existingPerson.FullName != user.FullName)
                        {
                            existingPerson.FullName = user.FullName;
                            modified = true;
                        }
                        //e-mail changed
                        else if (existingPerson.Email != user.Email)
                        {
                            existingPerson.Email = user.Email;
                            modified = true;
                        }
                        //phone number changed
                        else if (existingPerson.PhoneNumber != user.PhoneNumber)
                        {
                            existingPerson.PhoneNumber = user.PhoneNumber;
                            modified = true;
                        }
                        //departmant changed
                        else if (existingPerson.Dep != user.Dep)
                        {
                            existingPerson.Dep = user.Dep;
                            modified = true;
                        }
                        if (modified)
                            db.Entry(existingPerson).State = EntityState.Modified;
                    }
                    //remove non-existing users from db
                    var nonExistingIds = (from user in db.Persons.AsEnumerable()
                                          where adUsers.FirstOrDefault(u => u.AccountName == user.AccountName) == null
                                          select user.Id).ToArray();
                    if (nonExistingIds.Length > 0)
                    {
                        log.AppendLine("Удалены пользователи:");
                        foreach (var id in nonExistingIds)
                        {
                            var nonExistingUser = db.Persons.Find(id);
                            if (nonExistingUser == null)
                                continue;
                            foreach (var history in db.Histories
                                .Where(h => h.WhoTook.Id == nonExistingUser.Id || h.WhoGave.Id == nonExistingUser.Id).ToList())
                                db.Histories.Remove(history);
                            foreach (var comp in db.Computers.Where(c => c.Owner.Id == nonExistingUser.Id).ToList())
                            {
                                comp.Owner = null;
                                db.Entry(comp).State = EntityState.Modified;
                            }
                            db.Persons.Remove(nonExistingUser);
                            log.AppendLine(nonExistingUser.FullName);
                        }
                    }

                    db.SaveChanges();
                    if (newUsers.Count > 0)
                    {
                        log.AppendLine("Добавлены новые пользователи:");
                        foreach (var user in newUsers)
                            log.AppendLine(user);
                    }
                    if (log.Length > 0)
                        Task.Run(() => log.ToString().WriteToLogAsync(source:"Users"));
                }
                catch (Exception ex)
                {
                    var error = ex.Message + ex.InnerException?.Message + ex.InnerException?.InnerException?.Message;
                    Task.Run(() => error.WriteToLogAsync(EventLogEntryType.Error));
                }
            }
        }
        public static async void RefreshComputers()
        {
            using (var db = new InventoryModel())
            {
                try
                {
                    //foreach (var comp in db.Computers)
                    //{
                    //    comp.FillInventedData();
                    //    db.Entry(comp).State = EntityState.Modified;
                    //}
                    //db.SaveChanges();


                    var logNonEmpty = false;
                    var rootDir = new DirectoryInfo(@"\\rivs.org\it\ConfigReporting\ConfigReports");
                    var allComputers = new List<Computer>();        //all computers from aida reports
                    var log = new StringBuilder();
                    foreach (var dir in rootDir.GetDirectories().Where(dir => !dir.Name.Contains("SERVER-")))
                    {
                        var report = await Report.GetReportAsync(dir.Name);
                        if (report == null)
                            continue;
                        var comp = new Computer
                        {
                            ComputerName = report.CompName,
                            Cpu = report.Cpu,
                            Ram = report.Ram,
                            MotherBoard = report.MotherBoard,
                            VideoAdapter = report.VideoAdapter,
                            Software = report.Software,
                            Owner = await db.Persons.FirstOrDefaultAsync(p => p.AccountName == report.UserName),
                        };
                        allComputers.Add(comp);
                    }
                    //cleanup database from non-existing computers
                    var nonExistingCompNames = (from comp in db.Computers.AsEnumerable()
                                                where allComputers.FirstOrDefault(c => c.ComputerName == comp.ComputerName) == null
                                                select comp.ComputerName).ToArray();
                    if (nonExistingCompNames.Length > 0)
                    {
                        log.AppendLine("Удалены компьютеры:");
                        foreach (var comp in nonExistingCompNames)
                        {
                            var nonExisting = await db.Computers.FirstOrDefaultAsync(c => c.ComputerName == comp);
                            if (nonExisting == null)
                                continue;
                            db.Computers.Remove(nonExisting);
                            log.AppendLine(comp);
                            logNonEmpty = true;
                        }
                    }

                    log.AppendLine("Добавлены или изменены компьютеры:");
                    foreach (var comp in allComputers)
                    {
                        var existingComputer = await db.Computers.FirstOrDefaultAsync(c => c.ComputerName == comp.ComputerName);
                        if (existingComputer == null)
                        {
                            comp.FillInventedData();
                            db.Computers.Add(comp);
                            log.AppendLine(comp.ComputerName);
                            logNonEmpty = true;
                        }
                        else if (!existingComputer.Equals(comp))
                        {
                            existingComputer.CopyConfig(comp);
                            existingComputer.FillInventedData();
                            db.Entry(existingComputer).State = EntityState.Modified;
                            log.AppendLine(comp.ComputerName);
                            logNonEmpty = true;
                        }
                    }
                    await db.SaveChangesAsync();
                    if (logNonEmpty)
                        await log.ToString().WriteToLogAsync(source: "Computers");
                }
                catch (Exception ex)
                {
                    await ex.Message.WriteToLogAsync(EventLogEntryType.Error);
                }
            }
        }
        public static async Task WriteToLogAsync(this string log, EventLogEntryType type = EventLogEntryType.Information, string source = "Inventory")
        {
            const string logPath = "C:\\ProgramData\\Inventory\\";
            const string logFile = "Error.log";
            await Task.Run(() =>
            {
                try
                {
                    if (!EventLog.SourceExists(source))
                        EventLog.CreateEventSource(source, "InventorySiteLog");

                    using (var journal = new EventLog())
                    {
                        journal.Source = source;
                        journal.Log = "InventorySiteLog";
                        journal.WriteEntry(log, type);
                    }
                }
                catch (Exception ex) when (ex is SecurityException || ex is ObjectDisposedException)
                {
                    using (var outFile = new StreamWriter(Path.Combine(logPath, logFile), true))
                    {
                        outFile.WriteLineAsync(ex.Message + ex.InnerException);
                    }
                }
            });
        }
        public static string GetUserName(this string login)
        {
            using (var db = new InventoryModel())
            {
                var accountName = login.Substring(5, login.Length - 5);
                var person = db.Persons.FirstOrDefault(p => p.AccountName == accountName);
                return person != null ? person.FullName : login;
            }
        }
        public static string GetShortName(this string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;
            var nameParts = (fullName.Split(' '));
            var shortName = string.Empty;
            for (var i = 0; i < nameParts.Length; i++)
            {
                if (i == 0)
                    shortName = nameParts[i];
                else
                    shortName = shortName + " " + nameParts[i].First() + ".";
            }
            return shortName;
        }
        public static string GetCompName(string ip)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address))
                return string.Empty;
            var host = Dns.GetHostEntry(address);
            return host.HostName.Split('.').First();
        }
        public static bool IsIp(string ipString)
        {
            IPAddress address;
            return IPAddress.TryParse(ipString, out address);
        }
        public static bool IsNumber(string value)
        {
            int number;
            return int.TryParse(value, out number);
        }
        public static bool IsCartridgesOver(int printerId)
        {
            using (var db = new InventoryModel())
            {
                var printer = db.Printers.FirstOrDefault(p => p.Id == printerId);
                return printer != null && printer.Cartridges.Any(cartridge => cartridge.Quantity <= cartridge.MinQuantity);
            }
        }
        public static int CountPrinters(int cartridgeId)
        {
            using (var db = new InventoryModel())
            {
                return db.Printers.Count(p => p.Cartridges.FirstOrDefault(c => c.Id == cartridgeId) != null);
            }
        }
        public static int CountComputers(int personId)
        {
            using (var db = new InventoryModel())
            {
                return db.Computers.Count(c => c.Owner.Id == personId);
            }
        }
        public static int CountItemGrant(int id, int days)
        {
            using (var db = new InventoryModel())
            {
                return Enumerable.Sum(db.Histories
                    .Where(history => history.Recieved == false 
                    && history.Item.Id == id 
                    && DbFunctions.DiffDays(history.Date, DateTime.Now) <= days), 
                    history => history.Quantity);
            }
        }
        public static int CountUserGrant(int id)
        {
            using (var db = new InventoryModel())
            {
                return db.Histories.Count(h => h.Recieved == false && h.WhoTook.Id == id);
            }
        }
        public static int CountRecieve(int id, int days)
        {
            using (var db = new InventoryModel())
            {
                return Enumerable.Sum(db.Histories
                    .Where(history => history.Recieved
                    && history.Item.Id == id
                    && DbFunctions.DiffDays(history.Date, DateTime.Now) <= days),
                    history => history.Quantity);
            }
        }
        public static IEnumerable<ItemType> GetTypes()
        {
            using (var db = new InventoryModel())
            {
                //put "Прочее" to the end of list
                var list = db.ItemTypes.Where(t => t.Id != 10).OrderBy(t => t.Name).ToList();
                list.Add(db.ItemTypes.FirstOrDefault(t => t.Id == 10));
                return list;
            }
        }
        public static IEnumerable<Office> GetOffices()
        {
            using (var db = new InventoryModel())
            {
                var offices = new List<Office>
                {
                    //Zheleznovodskaya first
                    db.Offices.FirstOrDefault(o => o.Id == 1)
                };
                offices.AddRange(db.Offices.Where(o => o.Id != 1).OrderBy(o => o.Name));
                return offices;
            }
        }
        public static IEnumerable<Printer> GetPrinters(int cartridgeId)
        {
            using (var db = new InventoryModel())
            {
                return (from printer in db.Printers
                        from cartridge in printer.Cartridges
                        where cartridge.Id == cartridgeId
                        select printer).OrderBy(p => p.Name).ToList();
            }
        }
        public static IEnumerable<SelectListItem> SelectOffices()
        {
            return new SelectList(GetOffices(), "Id", "Name");
        }
        public static IEnumerable<SelectListItem> SelectColors()
        {
            return new SelectList(new Dictionary<string, string>
            {
                {"#000000", "Black"},
                {"#00FFFF", "Cyan"},
                {"#FF00FF", "Magenta"},
                {"#FFFF00", "Yellow"},
                {"#808080", "Gray"},
                {"#010101", "PGBK"}
            }, "Key", "Value");
        }
        public static IEnumerable<SelectListItem> SelectCartridges()
        {
            using (var db = new InventoryModel())
                return new SelectList(db.Items.Where(i => i.ItemType.Id == 11).ToList(), "Id", "Name");
        }
        public static IEnumerable<SelectListItem> SelectDepartments()
        {
            using (var db = new InventoryModel())
                return new SelectList(db.Departments.OrderBy(d => d.Name).ToList(), "Id", "Name");
        }
        public static IEnumerable<SelectListItem> SelectAttributes()
        {
            using (var db = new InventoryModel())
            {
                var attributes = db.ItemAttributes.OrderBy(a => a.Name).ToList();
                return new SelectList(attributes, "Id", "Name");
            }
        }
        public static IEnumerable<SelectListItem> SelectUsers(bool isInIt)
        {
            using (var db = new InventoryModel())
            {
                var people = isInIt
                    ? db.Persons.Where(p => p.Dep.Name == "Департамент информационных технологий").OrderBy(p => p.FullName).ToList()
                    : db.Persons.OrderBy(p => p.FullName).ToList();
                return new SelectList(people, "Id", "FullName");
            }
        }







        public static Dictionary<int, string> RequestUrgency = new Dictionary<int, string>
        {
            {0, "Срочно" },
            {1, "1 час" },
            {2, "2 часа" },
            {3, "4 часа" },
            {4, "В течении дня" }
        };

        public static Dictionary<int, string> RequestState = new Dictionary<int, string>
        {
            {0, "Новая" },
            {1, "В работе" },
            {2, "Завершена" }
        };

        public static Dictionary<int, string> RequestCategory = new Dictionary<int, string>
        {
            {0, "Программное обеспечение" },
            {1, "Оборудование" },
            {2, "Принтеры" },
            {3, "Интернет" },
            {4, "Почта" },
            {5, "Телефония" },
            {6, @"Файлы/папки" },
            {7, "Другое" }
        };
    }
}