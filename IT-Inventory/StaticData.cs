using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory
{
    public static class StaticData
    {
        public static async void RefreshUsers()
        {
            using (var db = new InventoryModel())
            {
                var adUsers = new List<Person>();
                using (var context = new PrincipalContext(ContextType.Domain, "rivs.org"))
                using (var group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, "Пользователи домена"))
                {
                    if (@group != null)
                    {
                        adUsers = (from p in @group.GetMembers(false)
                                   let entry = new DirectoryEntry("LDAP://DC=rivs,DC=org")
                                   select new DirectorySearcher(entry)
                                   {
                                       Filter = "(&(objectClass=person) (samaccountname=" + p.SamAccountName + "))"
                                   }
                            into mySearcher
                                   select mySearcher.FindOne().GetDirectoryEntry()
                            into resultEntry
                                   where resultEntry.Properties.Contains("l")
                                       && resultEntry.Properties["l"].Value.ToString() == "Санкт-Петербург"
                                       && resultEntry.Properties.Contains("DisplayName")
                                       && !resultEntry.Properties.Contains("st")
                                       && resultEntry.Properties.Contains("department")
                                   select new Person
                                   {
                                       FullName = resultEntry.Properties["DisplayName"].Value.ToString(),
                                       AccountName = resultEntry.Properties["sAMAccountName"].Value.ToString(),
                                       IsItUser = resultEntry.Properties["department"].Value.ToString() == "Департамент информационных технологий"
                                   }).ToList();
                    }
                }
                //add non-existing users to db
                foreach (var user in adUsers)
                {
                    var existingPerson = db.Persons.FirstOrDefault(p => p.AccountName == user.AccountName);
                    if (existingPerson == null)
                        db.Persons.Add(user);
                    else if (existingPerson.IsItUser != user.IsItUser)
                    {
                        existingPerson.IsItUser = user.IsItUser;
                        db.Entry(existingPerson).State = EntityState.Modified;
                    }
                    else if (existingPerson.FullName != user.FullName)
                    {
                        existingPerson.FullName = user.FullName;
                        db.Entry(existingPerson).State = EntityState.Modified;
                    }
                }
                //remove non-existing users from db
                var dbUsers = db.Persons.ToList();
                var nonExisting = dbUsers.Where(user => adUsers.FirstOrDefault(p => p.AccountName == user.AccountName) == null);
                db.Persons.RemoveRange(nonExisting);
                await db.SaveChangesAsync();
            }
        }
        public static string GetUserName(string login)
        {
            using (var db = new InventoryModel())
            {
                //return login;
                var accountName = login.Substring(5, login.Length - 5);
                var person = db.Persons.FirstOrDefault(p => p.AccountName == accountName);
                return person != null ? person.FullName : login;
            }
        }
        public static bool CheckIp(string ipString)
        {
            IPAddress address;
            return IPAddress.TryParse(ipString, out address);
        }
        public static IEnumerable<ItemType> GetTypes()
        {
            List<ItemType> list;
            using (var db = new InventoryModel())
            {
                //put "Прочее" to the end of list
                list = db.ItemTypes.Where(t => t.Id != 10).OrderBy(t => t.Name).ToList();
                list.Add(db.ItemTypes.FirstOrDefault(t => t.Id == 10));
            }
            return list;
        }
        public static IEnumerable<Office> GetOffices()
        {
            var offices = new List<Office>();
            using (var db = new InventoryModel())
            {
                offices.Add(db.Offices.FirstOrDefault(o => o.Id == 1));
                offices.AddRange(db.Offices.Where(o => o.Id != 1).OrderBy(o => o.Name));
            }
            return offices;
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
            List<ItemAttribute> attributes;
            using (var db = new InventoryModel())
                attributes = db.ItemAttributes.OrderBy(a => a.Name).ToList();
            return new SelectList(attributes, "Id", "Name");
        }
        public static IEnumerable<SelectListItem> SelectUsers(bool isInIt)
        {
            List<Person> people;
            using (var db = new InventoryModel())
                people = isInIt
                    ? db.Persons.Where(p => p.IsItUser).OrderBy(p => p.FullName).ToList()
                    : db.Persons.OrderBy(p => p.FullName).ToList();
            return new SelectList(people, "Id", "FullName");
        }
    }
}