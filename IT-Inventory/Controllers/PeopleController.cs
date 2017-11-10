using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModels;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class PeopleController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: People
        public ActionResult Index(string letter, int page = 1)
        {
            var model = new PeopleIndexViewModel();
            //sync users table with AD
            //if (updateList)
            //{
            //    StaticData.RefreshUsers();
            //    model.IsRefreshed = true;
            //}
            //else
            //    model.IsRefreshed = false;

            List<Person> items;
            if (letter == null)
                items = _db.Persons.OrderBy(p => p.FullName).ToList();
            else
                items = _db.Persons.AsEnumerable()
                        .Where(p => p.FullName.First() == letter.First())
                        .OrderBy(p => p.FullName)
                        .ToList();
            var pager = new Pager(items.Count, page, 8);
            model.People = items.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize);
            model.Pager = pager;
            model.FirstLetters = _db.Persons.AsEnumerable().Select(p => p.FullName.First()).Distinct().OrderBy(c => c).ToArray();
            model.Letter = letter;
            return View(model);
        }

        // GET: People/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = await _db.Persons.FindAsync(id);
            if (user == null)
                return HttpNotFound();
            return View(user);
        }


        // GET: People/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = await _db.Persons.FindAsync(id);
            if (user == null)
                return HttpNotFound();
            return View(user);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var user = await _db.Persons.FindAsync(id);
            if (user == null)
                return HttpNotFound();
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(user);
            }
            foreach (var history in _db.ComputerHistory.Where(h => h.HistoryComputerOwner.Id == id).ToList())
                history.HistoryComputerOwner = null;
            _db.Persons.Remove(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult GetPeopleJson()
        {
            var people = new List<PersonStruct>();
            foreach (var person in _db.Persons.AsEnumerable())
            {
                var user = new PersonStruct
                {
                    FullName = person.FullName,
                    AccountName = person.AccountName,
                    Email = person.Email,
                    Department = person.Dep.Name
                };
                int.TryParse(person.PhoneNumber, out user.PhoneNumber);
                people.Add(user);
            }
            return Json(people, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetPicture(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (id == 0)
                return File(Path.Combine(HostingEnvironment.MapPath(@"~/Content"), "noPhoto.jpg"), "image/jpeg");
            var user = await _db.Persons.FindAsync(id);
            if (user == null)
                return HttpNotFound();
            if (user.PhotoBytes != null)
                return File(user.PhotoBytes, "image/jpeg");
            return File(Path.Combine(HostingEnvironment.MapPath(@"~/Content"), "noPhoto.jpg"), "image/jpeg");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
