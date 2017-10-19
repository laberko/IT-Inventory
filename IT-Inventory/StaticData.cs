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
        public static async void RefreshUsers()
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
                    var startTime = DateTime.Now;
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
                                    resultEntry.Properties.Contains("title") &&
                                    resultEntry.Properties.Contains("company"))
                                {
                                    //get the first department
                                    var dep1Name = resultEntry.Properties["company"].Value.ToString();
                                    var dep1 = db.Departments.FirstOrDefault(d => d.Name == dep1Name);
                                    if (dep1 == null)
                                    {
                                        var newDep = new Department
                                        {
                                            Name = dep1Name
                                        };
                                        db.Departments.Add(newDep);
                                        dep1 = newDep;
                                    }
                                    //user has a second department
                                    Department dep2 = null;
                                    if (resultEntry.Properties.Contains("division"))
                                    {
                                        var dep2Name = resultEntry.Properties["division"].Value.ToString();
                                        dep2 = db.Departments.FirstOrDefault(d => d.Name == dep2Name);
                                        if (dep2 == null)
                                        {
                                            var newDep = new Department
                                            {
                                                Name = dep2Name
                                            };
                                            db.Departments.Add(newDep);
                                            dep2 = newDep;
                                        }
                                    }
                                        //index in department
                                        int depIndex;
                                        DateTime birthday;
                                        adUsers.Add(new Person
                                        {
                                        FullName = resultEntry.Properties["DisplayName"].Value.ToString(),
                                        AccountName = resultEntry.Properties["sAMAccountName"].Value.ToString(),
                                        Email = resultEntry.Properties.Contains("mail")
                                            ? resultEntry.Properties["mail"].Value.ToString()
                                            : string.Empty,
                                        PhoneNumber = resultEntry.Properties.Contains("telephoneNumber")
                                            ? resultEntry.Properties["telephoneNumber"].Value.ToString()
                                            : string.Empty,
                                        Position = resultEntry.Properties["title"].Value.ToString(),
                                        Birthday = (resultEntry.Properties.Contains("description")
                                                    && DateTime.TryParse(
                                                        resultEntry.Properties["description"].Value.ToString(),
                                                        out birthday))
                                            ? birthday as DateTime?
                                            : null,
                                        CreationDate = resultEntry.Properties.Contains("whenCreated")
                                            ? resultEntry.Properties["whenCreated"].Value as DateTime?
                                            : null,
                                        Dep = dep1,
                                        //index in the first department
                                        Dep1Index = (resultEntry.Properties.Contains("postalCode")
                                                        && int.TryParse(
                                                            resultEntry.Properties["postalCode"].Value.ToString(),
                                                            out depIndex))
                                            ? depIndex
                                            : 0,
                                        Dep2 = dep2,
                                        //index in the second department (if not null)
                                        Dep2Index = (dep2 != null
                                                        && resultEntry.Properties.Contains("countryCode")
                                                        && int.TryParse(
                                                            resultEntry.Properties["countryCode"].Value.ToString(),
                                                            out depIndex))
                                            ? depIndex
                                            : 0,
                                        //user's group in department
                                        Group = resultEntry.Properties["department"].Value.ToString(),
                                        //picture in bytes
                                        PhotoBytes = resultEntry.Properties.Contains("thumbnailPhoto")
                                            ? (byte[])resultEntry.Properties["thumbnailPhoto"].Value
                                            : null
                                    });
                                }
                            }
                        }
                    }

                    //add non-existing users to db or replace changed fields
                    var usersString = new StringBuilder();
                    foreach (var user in adUsers)
                    {
                        //find ad user in db
                        var existingPerson = await db.Persons.FirstOrDefaultAsync(p => p.AccountName == user.AccountName);
                        var modified = false;
                        //new user
                        if (existingPerson == null)
                        {
                            db.Persons.Add(user);
                            newUsers.Add(user.FullName);
                        }
                        //user exists - check if something changed
                        else
                        {
                            //full name changed
                            if (existingPerson.FullName != user.FullName)
                            {
                                existingPerson.FullName = user.FullName;
                                modified = true;
                            }
                            //"existing" status changed changed
                            if (existingPerson.NonExisting != user.NonExisting)
                            {
                                existingPerson.NonExisting = user.NonExisting;
                                modified = true;
                            }
                            //e-mail changed
                            if (existingPerson.Email != user.Email)
                            {
                                existingPerson.Email = user.Email;
                                modified = true;
                            }
                            //phone number changed
                            if (existingPerson.PhoneNumber != user.PhoneNumber)
                            {
                                existingPerson.PhoneNumber = user.PhoneNumber;
                                modified = true;
                            }
                            //position changed
                            if (existingPerson.Position != user.Position)
                            {
                                existingPerson.Position = user.Position;
                                modified = true;
                            }
                            //birthday changed
                            if (existingPerson.Birthday != user.Birthday)
                            {
                                existingPerson.Birthday = user.Birthday;
                                modified = true;
                            }
                            //creation date changed
                            if (existingPerson.CreationDate != user.CreationDate)
                            {
                                existingPerson.CreationDate = user.CreationDate;
                                modified = true;
                            }
                            //the first department changed
                            if (existingPerson.Dep != user.Dep)
                            {
                                existingPerson.Dep = user.Dep;
                                modified = true;
                            }
                            //the first department index changed
                            if (existingPerson.Dep1Index != user.Dep1Index)
                            {
                                existingPerson.Dep1Index = user.Dep1Index;
                                modified = true;
                            }
                            //the second department changed
                            if (existingPerson.Dep2 != user.Dep2)
                            {
                                existingPerson.Dep2 = user.Dep2;
                                modified = true;
                            }
                            //the second department index changed
                            if (existingPerson.Dep2Index != user.Dep2Index)
                            {
                                existingPerson.Dep2Index = user.Dep2Index;
                                modified = true;
                            }
                            //group changed
                            if (existingPerson.Group != user.Group)
                            {
                                existingPerson.Group = user.Group;
                                modified = true;
                            }
                            //picture changed
                            if (existingPerson.PhotoBytes != user.PhotoBytes)
                            {
                                existingPerson.PhotoBytes = user.PhotoBytes;
                                modified = true;
                            }
                        }

                        if (modified)
                            db.Entry(existingPerson).State = EntityState.Modified;

                        //add string to file
                        if (user.Position != "-")
                            usersString.AppendLine(user.AccountName + ";" +
                                         user.FullName + ";" +
                                         user.Email + ";" +
                                         user.Dep.Name + ";" +
                                         user.Position + ";" +
                                         user.PhoneNumber + ";");
                    }

                    const string path = @"\\rivs.org\it\ConfigReporting\users.txt";
                    File.WriteAllText(path, usersString.ToString());

                    //non-existing users
                    var nonExistingIds = (from user in db.Persons.AsEnumerable()
                                          where !user.NonExisting && adUsers.FirstOrDefault(u => u.AccountName == user.AccountName) == null
                                          select user.Id).ToArray();
                    if (nonExistingIds.Length > 0)
                    {
                        log.AppendLine("Несуществующие (уволенные) пользователи:");
                        foreach (var id in nonExistingIds)
                        {
                            var nonExistingUser = db.Persons.Find(id);
                            if (nonExistingUser == null)
                                continue;
                            nonExistingUser.NonExisting = true;
                            db.Entry(nonExistingUser).State = EntityState.Modified;
                            log.AppendLine(nonExistingUser.FullName);
                        }

                        //log.AppendLine("Удалены пользователи:");
                        //foreach (var id in nonExistingIds)
                        //{
                        //    var nonExistingUser = db.Persons.Find(id);
                        //    if (nonExistingUser == null)
                        //        continue;
                        //    foreach (var history in db.Histories
                        //        .Where(h => h.WhoTook.Id == nonExistingUser.Id || h.WhoGave.Id == nonExistingUser.Id).ToList())
                        //        db.Histories.Remove(history);
                        //    foreach (var comp in db.Computers.Where(c => c.Owner.Id == nonExistingUser.Id).ToList())
                        //    {
                        //        comp.Owner = null;
                        //        db.Entry(comp).State = EntityState.Modified;
                        //    }
                        //    db.Persons.Remove(nonExistingUser);
                        //    log.AppendLine(nonExistingUser.FullName);
                        //}
                    }

                    await db.SaveChangesAsync();
                    if (newUsers.Count > 0)
                    {
                        log.AppendLine("Добавлены новые пользователи:");
                        foreach (var user in newUsers)
                            log.AppendLine(user);
                    }
                    if (log.Length == 0)
                        return;
                    log.AppendLine("Синхронизация пользователей заняла " + (int)((DateTime.Now - startTime).TotalSeconds) + " секунд.");
                    await log.ToString().WriteToLogAsync(source: "Users");
                }
                catch (Exception ex)
                {
                    ex.WriteToLogAsync(source: "Users");
                }
            }
        }
        public static async void RefreshComputers()
        {
            using (var db = new InventoryModel())
            {
                try
                {
                    var startTime = DateTime.Now;
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
                            IsNotebook = report.IsNotebook,
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
                            UpdateDate = DateTime.Now,
                            MbId = report.MbId
                        };
                        realComputers.Add(comp);
                    }

                    //searching for duplicate computers with the same ID
                    var duplicateMbIds = realComputers.GroupBy(c => c.MbId).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
                    if (duplicateMbIds.Length > 0)
                    {
                        log.AppendLine("Дублирующиеся записи компьютеров:");
                        foreach (var duplicateId in duplicateMbIds)
                        {
                            //take the newest duplicate comp from list which probably is the existing one
                            var existingRealComp = realComputers.Where(c => c.MbId == duplicateId).OrderByDescending(c => c.LastReportDate).FirstOrDefault();
                            if (existingRealComp == null)
                                continue;

                            //the rest (nonexisting) computers from list
                            var nonExistingComputers = realComputers.Where(c => c.MbId == duplicateId && c.ComputerName != existingRealComp.ComputerName).ToArray();

                            log.AppendLine(nonExistingComputers.Length == 1
                                ? "  (вероятно, был переименован)"
                                : "  (вероятно, не-уникальные ID материнских плат)");
                            log.AppendLine(" - новейшая запись: " + existingRealComp.ComputerName + " (ID: " + existingRealComp.MbId + ")");
                            logNonEmpty = true;

                            //find the newest duplicate (existing) comp in db
                            var existingCompInDb = await db.Computers.Where(c => c.MbId == duplicateId && c.ComputerName == existingRealComp.ComputerName).FirstOrDefaultAsync();
                            //the newest was not found in db means it is the fresh copy of a renamed computer so we need to add it
                            if (existingCompInDb == null)
                            {
                                db.Computers.Add(existingRealComp);
                                log.AppendLine(" - добавлен в базу: " + existingRealComp.ComputerName + " (ID: " + existingRealComp.MbId + ")");
                                db.SaveChanges();
                                existingCompInDb = await db.Computers.Where(c => c.MbId == duplicateId && c.ComputerName == existingRealComp.ComputerName).FirstOrDefaultAsync();
                                if (existingCompInDb == null)
                                    continue;
                                if (nonExistingComputers.Length != 0)
                                {
                                    var oldName = nonExistingComputers.OrderByDescending(c => c.LastReportDate).First().ComputerName;
                                    db.ComputerHistory.Add(existingCompInDb.NewHistory("Переименован", oldName: oldName));
                                }
                            }

                            //for each of the rest (nonexisting) computers in list
                            foreach (var oldComp in nonExistingComputers)
                            {
                                log.AppendLine(" - дубликат: " + oldComp.ComputerName + " (ID: " + oldComp.MbId + ")");
                                //remove nonexisting computer from list
                                realComputers.Remove(oldComp);
                                //remove the folder containing report
                                var reportDir = new DirectoryInfo(Path.Combine(rootPath, oldComp.ComputerName));
                                reportDir.Delete(true);

                                var nonExistingCompInDb = await db.Computers.FirstOrDefaultAsync(c => c.ComputerName == oldComp.ComputerName);
                                if (nonExistingCompInDb == null)
                                    continue;
                                //transfer support requests to the existing computer
                                foreach (var request in nonExistingCompInDb.SupportRequests.ToArray())
                                {
                                    request.FromComputer = existingCompInDb;
                                    existingCompInDb.SupportRequests.Add(request);
                                    nonExistingCompInDb.SupportRequests.Remove(request);
                                    db.Entry(request).State = EntityState.Modified;
                                }
                                //transfer history to the existing computer
                                foreach (var history in db.ComputerHistory.Where(h => h.HistoryComputer.Id == nonExistingCompInDb.Id).ToArray())
                                {
                                    history.HistoryComputer = existingCompInDb;
                                    db.Entry(history).State = EntityState.Modified;
                                }
                                db.Entry(existingCompInDb).State = EntityState.Modified;
                            }
                        }
                        db.SaveChanges();
                    }

                    //cleanup database from non-existing computers (including duplicates)
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
                            log.AppendLine(" - " + compName + " (ID: " + nonExisting.MbId + ")");
                            logNonEmpty = true;
                            db.Computers.Remove(nonExisting);
                        }
                        db.SaveChanges();
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
                            comp.FillFixedData();
                            db.Computers.Add(comp);
                            db.ComputerHistory.Add(comp.NewHistory());
                            log.AppendLine(" - новый: " + comp.ComputerName + " (ID: " + comp.MbId + ")");
                            logNonEmpty = true;
                            continue;
                        }
                        //computer found but has changes in configuration
                        if (!existingComputer.Equals(comp))
                        {
                            var changes = existingComputer.CopyConfig(comp);
                            //write changes to db only if we have some info about them
                            if (!string.IsNullOrEmpty(changes[0]) && changes[0].Length >= 4)
                            {
                                db.ComputerHistory.Add(existingComputer.NewHistory(changes[0], changes[1], changes[2]));
                                log.AppendLine(" - изменен: " + comp.ComputerName + " (" + changes[0] + ")");
                                logNonEmpty = true;
                            }
                            db.Entry(existingComputer).State = EntityState.Modified;
                        }
                        //fixed computer data is not filled
                        if (existingComputer.FillFixedData())
                            db.Entry(existingComputer).State = EntityState.Modified;
                    }
                    db.SaveChanges();

                    //find duplicate names in db
                    var duplicateNamesInDb = db.Computers.AsEnumerable().GroupBy(c => c.ComputerName).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
                    if (duplicateNamesInDb.Length > 0)
                    {
                        log.AppendLine("В базе обнаружены компьютеры с одинаковыми именами:");
                        foreach (var name in duplicateNamesInDb)
                        {
                            var duplicates = db.Computers.AsEnumerable().Where(c => c.ComputerName == name).OrderByDescending(c => c.UpdateDate).ToList();
                            foreach (var comp in duplicates)
                                log.AppendLine(" - " + name + " (ID: " + comp.MbId + ")");
                            foreach (var comp in duplicates.Skip(1))
                            {
                                var compInDb = await db.Computers.FindAsync(comp.Id);
                                if (compInDb != null)
                                    db.Computers.Remove(compInDb);
                            }
                        }
                        logNonEmpty = true;
                        db.SaveChanges();
                    }

                    if (!logNonEmpty)
                        return;
                    log.AppendLine("Синхронизация компьютеров заняла " + (int)((DateTime.Now - startTime).TotalSeconds) + " секунд.");
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
            if (nameParts[0] == "Комната")
                return fullName;
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
        public static string ShortVideoAdapterName(this string adapterName)
        {
            return adapterName.Replace("(Microsoft Corporation - WDDM 1.0)", "")
                .Replace("(Microsoft Corporation WDDM 1.1)", "")
                .Replace("(Microsoft Corporation - WDDM)", "")
                .Replace("(Microsoft Corporation - WDDM v1.1)","")
                .Replace("( - WDDM 1.1)", "")
                .Replace("( - WDDM 1.0)", "")
                .Replace("Express Chipset Family", "")
                .Replace("Series", "");
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
                    ? db.Persons.Where(p => !p.NonExisting && p.Dep.Name == "Департамент информационных технологий").OrderBy(p => p.FullName).ToList()
                    : db.Persons.Where(p => !p.NonExisting).OrderBy(p => p.FullName).ToList();
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