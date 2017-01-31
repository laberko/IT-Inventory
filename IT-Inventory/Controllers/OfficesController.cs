using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModels;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class OfficesController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Offices
        public async Task<ActionResult> Index()
        {
            var offices = await _db.Offices.ToListAsync();
            return View(offices.OrderBy(o => o.Name));
        }

        // GET: Offices/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var office = await _db.Offices.FindAsync(id);
            if (office == null)
                return HttpNotFound();
            return View(office);
        }

        // GET: Offices/Create
        public ActionResult Create()
        {
            return View(new OfficeViewModel());
        }

        // POST: Offices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OfficeViewModel office)
        {
            if (!ModelState.IsValid)
                return View(office);
            //office with such name found in db
            if (_db.Offices.Any(o => o.Name == office.Name))
                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            var newOffice = new Office
            {
                Name = office.Name
            };
            _db.Offices.Add(newOffice);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Offices/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var office = await _db.Offices.FindAsync(id);
            if (office == null)
                return HttpNotFound();
            return View(new OfficeViewModel
            {
                Id = (int)id,
                Name = office.Name
            });
        }

        // POST: Offices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(OfficeViewModel office)
        {
            if (!ModelState.IsValid)
                return View(office);
            var editOffice = await _db.Offices.FindAsync(office.Id);
            editOffice.Name = office.Name;
            _db.Entry(editOffice).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Offices/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var office = await _db.Offices.FindAsync(id);
            if (office == null)
                return HttpNotFound();
            return View(office);
        }

        // POST: Offices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var office = await _db.Offices.FindAsync(id);
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(office);
            }
            _db.Offices.Remove(office);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
