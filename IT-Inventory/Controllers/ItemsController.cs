using System;
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
    public class ItemsController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Items
        public async Task<ActionResult> Index(int? id)
        {
            switch (id)
            {
                case null:
                    return View(await _db.Items.OrderBy(i => i.Name).ToListAsync());
                //trip notebooks
                case 8:
                    return View("IndexOfNotebook", await _db.Items.Where(i => i.ItemType.Id == id).OrderBy(i => i.Name).ToListAsync());
                default:
                    var type = await _db.ItemTypes.FindAsync(id);
                    ViewBag.TypeName = type.Name;
                    ViewBag.TypeId = type.Id;
                    return View("IndexOfType", await _db.Items.Where(i => i.ItemType.Id == id).OrderBy(i => i.Name).ToListAsync());
            }
        }

        // GET: Items/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            return View(item);
        }

        // GET: Items/Create
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var itemType = await _db.ItemTypes.FindAsync(id);
            if (itemType == null)
                return HttpNotFound();
            var itemModel = new ItemViewModel
            {
                ItemTypeId = (int)id,
                ItemTypeName = itemType.Name
            };
            //add collection of viewmodels containing attribute-value pairs to the main viewmodel
            foreach (var attribute in itemType.Attributes.OrderBy(a => a.Name))
                itemModel.AttributeValues.Add(new ItemAttributeValueViewModel
                {
                    AttributeId = attribute.Id,
                    Name = attribute.Name,
                    Value = string.Empty
                });
            return View(itemModel);
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            var newItem = new Item
            {
                Name = item.Name,
                Quantity = item.Quantity,
                ItemType = await _db.ItemTypes.FindAsync(item.ItemTypeId)
            };
            _db.Items.Add(newItem);
            var newHistory = new History
            {
                Recieved = true,
                Quantity = item.Quantity,
                Date = DateTime.Now,
                Item = newItem
            };
            _db.Histories.Add(newHistory);
            newItem.Histories.Add(newHistory);
            foreach (var attributeValue in item.AttributeValues)
            {
                var newValue = new ItemAttributeValue
                {
                    Attribute = await _db.ItemAttributes.FindAsync(attributeValue.AttributeId),
                    ParentItem = newItem,
                    Value = attributeValue.Value
                };
                newItem.AttributeValues.Add(newValue);
                _db.ItemAttributeValues.Add(newValue);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            var itemModel = new ItemViewModel
            {
                Id = (int) id,
                Name = item.Name,
                Quantity = item.Quantity,
                ItemTypeName = item.ItemType.Name,
                ItemTypeId = item.ItemType.Id
            };
            foreach (var attribute in item.ItemType.Attributes.OrderBy(a => a.Name))
            {
                //get attributes from item type: if it exists in found item - add existing to viewmodel, if not - create new
                var existingAttributeValue = item.AttributeValues.FirstOrDefault(a => a.Attribute.Id == attribute.Id);
                if (existingAttributeValue == null)
                {
                    itemModel.AttributeValues.Add(new ItemAttributeValueViewModel
                    {
                        Id = null,
                        Name = attribute.Name,
                        Value = string.Empty,
                        AttributeId = attribute.Id
                    });
                }
                else
                {
                    itemModel.AttributeValues.Add(new ItemAttributeValueViewModel
                    {
                        Id = existingAttributeValue.Id,
                        Name = existingAttributeValue.Attribute.Name,
                        Value = existingAttributeValue.Value,
                        AttributeId = attribute.Id
                    });
                }
            }
            return View(itemModel);
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            var editItem = await _db.Items.FindAsync(item.Id);
            if (editItem == null)
                return HttpNotFound();
            editItem.Name = item.Name;
            editItem.Quantity = item.Quantity;
            foreach (var attributeValue in item.AttributeValues)
            {
                var editValue = await _db.ItemAttributeValues.FindAsync(attributeValue.Id);
                //new attribute-value pair
                if (editValue == null)
                {
                    var newAttributeValue = new ItemAttributeValue
                    {
                        Attribute = await _db.ItemAttributes.FindAsync(attributeValue.AttributeId),
                        ParentItem = editItem,
                        Value = attributeValue.Value
                    };
                    _db.ItemAttributeValues.Add(newAttributeValue);
                    editItem.AttributeValues.Add(newAttributeValue);
                }
                //existing attribute-value pair
                else
                {
                    editValue.Value = attributeValue.Value;
                    _db.Entry(editValue).State = EntityState.Modified;
                }
            }
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/Recieve/5
        public async Task<ActionResult> Recieve(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            var itemModel = new ItemViewModel
            {
                Id = (int)id,
                Name = item.Name,
                Quantity = 1
            };
            return View(itemModel);
        }

        // POST: Items/Recieve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Recieve(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            var editItem = await _db.Items.FindAsync(item.Id);
            if (editItem == null)
                return HttpNotFound();
            editItem.Quantity += item.Quantity;
            var newHistory = new History
            {
                Recieved = true,
                Date = DateTime.Now,
                Item = editItem,
                Quantity = item.Quantity
            };
            _db.Histories.Add(newHistory);
            editItem.Histories.Add(newHistory);
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/RecieveOne/5      - for trip notebooks comeback
        public ActionResult RecieveOneNotebook(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = _db.Items.Find(id);
            item.Quantity++;
            _db.Entry(item).State = EntityState.Modified;
            _db.SaveChanges();
            return View("IndexOfNotebook", _db.Items.Where(i => i.ItemType.Id == 8).OrderBy(i => i.Name).ToList());
        }

        // GET: Items/Grant/5
        public async Task<ActionResult> Grant(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            var itemModel = new ItemViewModel
            {
                Id = (int)id,
                Name = item.Name,
                Quantity = 1
            };
            return View(itemModel);
        }

        // POST: Items/Grant/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Grant(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            var editItem = await _db.Items.FindAsync(item.Id);
            if (editItem == null)
                return HttpNotFound();
            if (editItem.Quantity < item.Quantity)
            {
                ModelState.AddModelError(string.Empty, "Нельзя выдать больше, чем есть в наличии (" + editItem.Quantity + ")!");
                return View(item);
            }
            var whoGave = await _db.Persons.FindAsync(item.WhoGaveId);
            var whoTook = await _db.Persons.FindAsync(item.WhoTookId);
            if (whoGave == null || whoTook == null)
                return View(item);
            editItem.Quantity -= item.Quantity;
            var newHistory = new History
            {
                Recieved = false,
                Date = DateTime.Now,
                Item = editItem,
                Quantity = item.Quantity,
                WhoGave = whoGave,
                WhoTook = whoTook
            };
            _db.Histories.Add(newHistory);
            editItem.Histories.Add(newHistory);
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Items.FindAsync(id);
            //if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            //{
            //    ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
            //    return View(item);
            //}
            _db.Items.Remove(item);
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
