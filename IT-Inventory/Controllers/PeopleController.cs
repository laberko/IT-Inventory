using System.Collections.Generic;
using System.Linq;
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
        public ActionResult Index(string letter, bool updateList = false, int page = 1)
        {
            var model = new PeopleIndexViewModel();
            //sync users table with AD
            if (updateList)
            {
                StaticData.RefreshUsers();
                model.IsRefreshed = true;
            }
            else
                model.IsRefreshed = false;

            var items = letter == null 
                ? _db.Persons.OrderBy(p => p.FullName).ToList() 
                : _db.Persons.AsEnumerable().Where(p => p.FullName.First() == letter.First()).OrderBy(p => p.FullName).ToList();
            var pager = new Pager(items.Count, page, 16);
            model.People = items.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize);
            model.Pager = pager;
            model.FirstLetters = _db.Persons.AsEnumerable().Select(p => p.FullName.First()).Distinct().OrderBy(c => c).ToArray();
            model.Letter = letter;
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
