using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ActionResult> Index(bool updateList = false)
        {
            var model = new PeopleIndexViewModel
            {
                People = await _db.Persons.OrderBy(p => p.FullName).ToListAsync()
            };
            //sync users table with AD
            if (updateList)
            {
                StaticData.RefreshUsers();
                model.IsRefreshed = true;
            }
            else
                model.IsRefreshed = false;
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
