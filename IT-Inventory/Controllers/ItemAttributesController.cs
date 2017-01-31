using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class ItemAttributesController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: ItemAttributes
        public async Task<ActionResult> Index()
        {
            return View(await _db.ItemAttributes.OrderBy(i => i.Name).ToListAsync());
        }

        // GET: ItemAttributes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ItemAttributes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemAttribute itemAttribute)
        {
            if (!ModelState.IsValid)
                return View(itemAttribute);
            _db.ItemAttributes.Add(itemAttribute);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: ItemAttributes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var itemAttribute = await _db.ItemAttributes.FindAsync(id);
            if (itemAttribute == null)
                return HttpNotFound();
            return View(itemAttribute);
        }

        // POST: ItemAttributes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemAttribute itemAttribute)
        {
            if (!ModelState.IsValid)
                return View(itemAttribute);
            _db.Entry(itemAttribute).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: ItemAttributes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var itemAttribute = await _db.ItemAttributes.FindAsync(id);
            if (itemAttribute == null)
                return HttpNotFound();
            return View(itemAttribute);
        }

        // POST: ItemAttributes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var itemAttribute = await _db.ItemAttributes.FindAsync(id);
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(itemAttribute);
            }
            _db.ItemAttributes.Remove(itemAttribute);
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
