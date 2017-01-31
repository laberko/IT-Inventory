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
    public class ItemTypesController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: ItemTypes
        public async Task<ActionResult> Index()
        {
            return View(await _db.ItemTypes.OrderBy(i => i.Name).ToListAsync());
        }

        // GET: ItemTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var itemType = await _db.ItemTypes.FindAsync(id);
            if (itemType == null)
                return HttpNotFound();
            return View(itemType);
        }

        // GET: ItemTypes/Create
        public ActionResult Create()
        {
            return View(new ItemTypeViewModel());
        }

        // POST: ItemTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemTypeViewModel itemType)
        {
            if (!ModelState.IsValid)
                return View(itemType);
            _db.ItemTypes.Add(new ItemType
            {
                Name = itemType.Name,
                Attributes = itemType.AttributeIds
                    .Where(a => a != null)
                    .Distinct()
                    .Select(item => _db.ItemAttributes.FirstOrDefault(a => a.Id == item))
                    .ToList()
            });
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: ItemTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var itemType = await _db.ItemTypes.FindAsync(id);
            if (itemType == null)
                return HttpNotFound();
            return View(new ItemTypeViewModel
            {
                TypeId = (int) id,
                Name = itemType.Name,
                AttributeIds = itemType.Attributes.Select(a => a.Id).Cast<int?>().ToList(),
            });
        }

        // POST: ItemTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemTypeViewModel itemType)
        {
            if (!ModelState.IsValid)
                return View(itemType);
            var editItemType = await _db.ItemTypes.FindAsync(itemType.TypeId);
            if (editItemType == null)
                return HttpNotFound();
            editItemType.Name = itemType.Name;
            editItemType.Attributes.Clear();
            foreach (var attrId in itemType.AttributeIds.Where(a => a != null).Distinct())
            {
                var attribute = await _db.ItemAttributes.FindAsync(attrId);
                if (attribute != null)
                    editItemType.Attributes.Add(attribute);
            }
            _db.Entry(editItemType).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: ItemTypes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var itemType = await _db.ItemTypes.FindAsync(id);
            if (itemType == null)
                return HttpNotFound();
            return View(itemType);
        }

        // POST: ItemTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var itemType = await _db.ItemTypes.FindAsync(id);
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(itemType);
            }
            itemType.Attributes.Clear();
            _db.ItemTypes.Remove(itemType);
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
