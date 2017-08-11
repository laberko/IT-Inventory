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
using IT_Inventory.ViewModels;

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
                                    //add ituser for testing
                                    //|| resultEntry.Properties.Contains("DisplayName") && (resultEntry.Properties["DisplayName"].Value.ToString() == "ituser"))
                                {
                                    Department dep = null;
                                    if (resultEntry.Properties.Contains("company"))
                                    {
                                        var depName = resultEntry.Properties["company"].Value.ToString();
                                        dep = db.Departments.FirstOrDefault(d => d.Name == depName);
                                        if (dep == null)
                                        {
                                            var newDep = new Department
                                            {
                                                Name = depName
                                            };
                                            db.Departments.Add(newDep);
                                            dep = newDep;
                                        }
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
                    //var nonExistingIds = (from user in db.Persons.AsEnumerable()
                    //                      where adUsers.FirstOrDefault(u => u.AccountName == user.AccountName) == null
                    //                      select user.Id).ToArray();
                    //if (nonExistingIds.Length > 0)
                    //{
                    //    log.AppendLine("Удалены пользователи:");
                    //    foreach (var id in nonExistingIds)
                    //    {
                    //        var nonExistingUser = db.Persons.Find(id);
                    //        if (nonExistingUser == null)
                    //            continue;
                    //        foreach (var history in db.Histories
                    //            .Where(h => h.WhoTook.Id == nonExistingUser.Id || h.WhoGave.Id == nonExistingUser.Id).ToList())
                    //            db.Histories.Remove(history);
                    //        foreach (var comp in db.Computers.Where(c => c.Owner.Id == nonExistingUser.Id).ToList())
                    //        {
                    //            comp.Owner = null;
                    //            db.Entry(comp).State = EntityState.Modified;
                    //        }
                    //        db.Persons.Remove(nonExistingUser);
                    //        log.AppendLine(nonExistingUser.FullName);
                    //    }
                    //}

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
                    ex.WriteToLogAsync(source: "Users");
                    //var error = ex.Message + ex.InnerException?.Message + ex.InnerException?.InnerException?.Message;
                    //Task.Run(() => error.WriteToLogAsync(EventLogEntryType.Error, "Users"));
                }
            }
        }
        public static async void RefreshComputers()
        {
            using (var db = new InventoryModel())
            {
                try
                {
                    var logNonEmpty = false;
                    const string rootPath = @"\\rivs.org\it\ConfigReporting\ConfigReports";
                    var rootDir = new DirectoryInfo(rootPath);
                    var realComputers = new List<Computer>();        //all computers from aida reports
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
                            Hdd = report.Hdd,
                            MotherBoard = report.MotherBoard,
                            VideoAdapter = report.VideoAdapter,
                            Monitor = report.Monitor,
                            Software = report.Software,
                            Owner = await db.Persons.FirstOrDefaultAsync(p => p.AccountName == report.UserName),
                            LastReportDate = report.ReportDate,
                            MbId = report.MbId
                        };
                        realComputers.Add(comp);
                    }


                    var duplicateMbIds = realComputers.GroupBy(c => c.MbId).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
                    if (duplicateMbIds.Length > 0)
                    {
                        log.AppendLine("Дублирующиеся записи компьютеров:");
                        foreach (var id in duplicateMbIds)
                        {
                            //take the newest comp from db
                            var existingComp = await db.Computers.Where(c => c.MbId == id).OrderByDescending(c => c.LastReportDate).FirstOrDefaultAsync();
                            if (existingComp == null)
                                continue;
                            //for each of the rest (nonexisting) computers
                            foreach (var comp in realComputers.Where(c => c.MbId == id).OrderByDescending(c => c.LastReportDate).Skip(1).ToArray())
                            {
                                realComputers.Remove(comp);
                                var reportDir = new DirectoryInfo(Path.Combine(rootPath, comp.ComputerName));
                                reportDir.Delete(true);
                                var nonExistingComp = await db.Computers.FirstOrDefaultAsync(c => c.ComputerName == comp.ComputerName);
                                if (nonExistingComp == null)
                                    continue;
                                log.AppendLine(comp.ComputerName);
                                logNonEmpty = true;
                                //transfer support requests to the existing computer
                                foreach (var request in nonExistingComp.SupportRequests.ToArray())
                                {
                                    request.FromComputer = existingComp;
                                    existingComp.SupportRequests.Add(request);
                                    nonExistingComp.SupportRequests.Remove(request);
                                    db.Entry(request).State = EntityState.Modified;
                                }
                                //transfer history to the existing computer
                                foreach (var history in db.ComputerHistory.Where(h => h.HistoryComputer.Id == nonExistingComp.Id && h.Changes != "Новый").ToArray())
                                {
                                    history.HistoryComputer = existingComp;
                                    db.Entry(history).State = EntityState.Modified;
                                }
                            }
                            db.Entry(existingComp).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }

                    //cleanup database from non-existing computers
                    var nonExistingCompNames = (from comp in db.Computers.AsEnumerable()
                                                where realComputers.FirstOrDefault(c => c.ComputerName == comp.ComputerName) == null
                                                select comp.ComputerName).ToArray();
                    if (nonExistingCompNames.Length > 0)
                    {
                        log.AppendLine("Удалены несуществующие компьютеры:");
                        foreach (var compName in nonExistingCompNames)
                        {
                            var nonExisting = await db.Computers.FirstOrDefaultAsync(c => c.ComputerName == compName);
                            if (nonExisting == null)
                                continue;
                            foreach (var history in db.ComputerHistory.Where(h => h.HistoryComputer.ComputerName == compName).AsEnumerable())
                                db.ComputerHistory.Remove(history);
                            db.Computers.Remove(nonExisting);
                            log.AppendLine(compName);
                            logNonEmpty = true;
                        }
                    }

                    //add new or edit existing computers in db
                    log.AppendLine("Добавлены или изменены компьютеры:");
                    foreach (var comp in realComputers)
                    {
                        //find computer in db by name
                        var existingComputer = await db.Computers.FirstOrDefaultAsync(c => c.ComputerName == comp.ComputerName);
                        //computer not found - add new
                        if (existingComputer == null)
                        {
                            comp.FillInventedData();
                            db.Computers.Add(comp);
                            db.ComputerHistory.Add(comp.NewHistory());
                            log.AppendLine(comp.ComputerName);
                            logNonEmpty = true;
                        }
                        //computer found but has changes in configuration
                        else if (!existingComputer.Equals(comp))
                        {
                            //fill missing data
                            existingComputer.FillInventedData();
                            var changes = existingComputer.CopyConfig(comp);
                            //write changes to db only if we have some info about them
                            if (!string.IsNullOrEmpty(changes[0]) && changes[0].Length >= 4)
                            {
                                db.ComputerHistory.Add(existingComputer.NewHistory(changes[0], changes[1], changes[2]));
                                log.AppendLine(comp.ComputerName);
                                logNonEmpty = true;
                            }
                            db.Entry(existingComputer).State = EntityState.Modified;
                        }
                        //computer found but data is not full
                        else if (existingComputer.FillInventedData())
                        {
                            db.Entry(existingComputer).State = EntityState.Modified;
                        }



                    }
                    db.SaveChanges();
                    if (logNonEmpty)
                        await log.ToString().WriteToLogAsync(source: "Computers");
                }
                catch (Exception ex)
                {
                    ex.WriteToLogAsync(source: "Computers");
                }
            }
        }
        public static void SendUrgentItemsMail()
        {
            try
            {
                using (var db = new InventoryModel())
                {
                    //send mail only after 9:00 once in 3 days
                    if (DateTime.Now.Hour <= 9)
                        return;
                    foreach (var mail in db.SentMails.Where(m => m.Type == MailType.Inventory).OrderByDescending(m => m.Date))
                    {
                        var mailDate = mail.Date;
                        var span = (DateTime.Now - mailDate);
                        if (span.Days < 3)
                            return;
                    }
                }
                //we invoke mail sending by calling this url (SendUrgentItemsMail() in ItemController)
                const string url = @"http://inventory.rivs.org/Items/SendUrgentItemsMail";
                var req = (HttpWebRequest) WebRequest.Create(url);
                req.Method = "GET";
                req.AllowAutoRedirect = false;
                req.UseDefaultCredentials = true;
                req.BeginGetResponse(null, null);
            }
            catch (Exception ex)
            {
                ex.WriteToLogAsync(source: "Mailer");
            }
        }
        public static async void WriteToLogAsync(this Exception ex, EventLogEntryType type = EventLogEntryType.Error, string source = "Inventory")
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            if (ex.InnerException != null)
            {
                sb.AppendLine(ex.InnerException.Message);
                if (ex.InnerException.InnerException != null)
                    sb.AppendLine(ex.InnerException.InnerException.Message);
            }
            sb.AppendLine(ex.StackTrace);
            await sb.ToString().WriteToLogAsync(type, source);
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
        public static MvcHtmlString NewLine2Br(this HtmlHelper htmlHelper, string text)
        {
            if (string.IsNullOrEmpty(text))
                return MvcHtmlString.Create(text);
            var builder = new StringBuilder();
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    builder.Append("<br/>\n");
                builder.Append(HttpUtility.HtmlEncode(lines[i]));
            }
            return MvcHtmlString.Create(builder.ToString());
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
        public static string GetItemFullName(int id)
        {
            using (var db = new InventoryModel())
            {
                var item = db.Items.FirstOrDefault(i => i.Id == id);
                if (item == null)
                    return string.Empty;
                return item.ItemType.Name + " " + item.Name;
            }

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
        public static int CountCartridgePrinters(int cartridgeId)
        {
            using (var db = new InventoryModel())
            {
                return db.Printers.Count(p => p.Cartridges.FirstOrDefault(c => c.Id == cartridgeId) != null);
            }
        }
        public static int CountOfficePrinters(int officeId)
        {
            using (var db = new InventoryModel())
            {
                return db.Printers.Count(p => p.Department.Office.Id == officeId);
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
        public static string LastOwnerShortName(int itemId)
        {
            using (var db = new InventoryModel())
            {
                var item = db.Items.Find(itemId);
                if (item == null || item.Histories.All(h => h.Recieved))
                    return string.Empty;
                return item.Histories.Where(h => h.Recieved == false).OrderByDescending(h => h.Date).First().WhoTook.ShortName;
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
        public static IEnumerable<Office> GetOffices(bool nonEmptyStorage = false)
        {
            using (var db = new InventoryModel())
            {
                var offices = new List<Office>
                {
                    //Zheleznovodskaya first
                    db.Offices.FirstOrDefault(o => o.Id == 1)
                };
                offices.AddRange(nonEmptyStorage
                    ? db.Offices.Where(o => o.Id != 1 && db.Items.Any(i => i.Location == o)).OrderBy(o => o.Name)
                    : db.Offices.Where(o => o.Id != 1).OrderBy(o => o.Name));
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
        public static IEnumerable<SelectListItem> SelectOffices(int exceptionId = 0)
        {
            return exceptionId == 0 
                ? new SelectList(GetOffices(), "Id", "Name") 
                : new SelectList(GetOffices().Where(o => o.Id != exceptionId), "Id", "Name");
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
                return new SelectList(db.Items.Where(i => i.ItemType.Id == 11).OrderBy(i => i.Name).ToList(), "Id", "Name");
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
        public static IEnumerable<SelectListItem> SelectHardware(int category = 0)
        {
            using (var db = new InventoryModel())
            {
                var itemList = category == 0 
                    ? db.Items.Where(i => i.Quantity > 0).OrderBy(i => i.Name).ToList() 
                    : db.Items.Where(i => i.ItemType.Id == category && i.Quantity > 0).OrderBy(i => i.Name).ToList();
                return new SelectList(itemList, "Id", "NameQuantity");
            }
        }
        public static IEnumerable<SelectListItem> SelectHardwareTypes()
        {
            return new SelectList(GetTypes(), "Id", "Name");
        }
        public static async Task<SupportMailViewModel> GetSupportMailViewModel(int requestId)
        {
            using (var db = new InventoryModel())
            {
                var request = await db.SupportRequests.FindAsync(requestId);
                if (request == null)
                    return null;
                return new SupportMailViewModel
                {
                    Id = requestId,
                    From = request.From == null ? string.Empty : request.From.FullName,
                    FromMail = request.From == null ? string.Empty : request.From.Email,
                    To = request.To == null ? string.Empty : request.To.FullName,
                    ToMail = request.To == null ? string.Empty : request.To.Email,
                    Text = request.Text,
                    FromComputer = request.FromComputer == null ? string.Empty : request.FromComputer.ComputerName,
                    DateCreated = request.CreationTime.ToString("f"),
                    DateStarted = request.StartTime?.ToString("f") ?? string.Empty,
                    DateFinished = request.FinishTime?.ToString("f") ?? string.Empty,
                    Urgency = request.Urgency,
                    Category = request.Category,
                    State = request.State,
                    SoftwareInstalled = request.SoftwareInstalled,
                    SoftwareRemoved = request.SoftwareRemoved,
                    SoftwareRepaired = request.SoftwareRepaired,
                    SoftwareUpdated = request.SoftwareUpdated,
                    HardwareInstalled = GetItemFullName(request.HardwareId),
                    //HardwareReplaced = request.HardwareReplaced,
                    OtherActions = request.OtherActions,
                    Comment = request.Comment,
                    Mark = request.Mark,
                    FeedBack = request.FeedBack,
                    ImagePathLastPart = request.File != null && request.File.IsImage ? request.FileNameLastPart : string.Empty,
                    FilePath = request.File != null && !request.File.IsImage ? request.File.Path : string.Empty
                };
            }
        }

        public static Dictionary<int, string> RequestUrgency = new Dictionary<int, string>
        {
            {4, "В течении дня" },
            {0, "Срочно" },
            {1, "1 час" },
            {2, "2 часа" },
            {3, "4 часа" }
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
            {6, "Файлы/папки" },
            {7, "1C" },
            {8, "Другое" }
        };

        public enum MailRecipients
        {
            AllSupportUsers,
            SupportUser,
            User
        };

        public enum RequestStatus
        {
            NewFromUser,
            NewFromIt,
            EditByUser,
            EditByIt,
            Accepted,
            Finished
        };

        public enum MailType
        {
            Support,
            Inventory
        };
    }
}