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
        // list of all items (id==null) or items of specific type
        public async Task<ActionResult> Index(int? id)
        {
            switch (id)
            {
                // all items
                case null:
                    return View(await _db.Items.OrderBy(i => i.ItemType.Name).ThenBy(i => i.Name).ToListAsync());
                // trip notebooks
                case 8:
                    return View("IndexOfNotebook", await _db.Items.Where(i => i.ItemType.Id == id).OrderBy(i => i.Name).ToListAsync());
                // items of type
                default:
                    var type = await _db.ItemTypes.FindAsync(id);
                    ViewBag.TypeName = type.Name;
                    ViewBag.TypeId = type.Id;
                    return View("IndexOfType", await _db.Items.Where(i => i.ItemType.Id == id).OrderBy(i => i.Name).ToListAsync());
            }
        }

        // GET: Items/Details/5
        // detailed info for an item
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            //special data for cartridges
            if (item.ItemType.Id == 11)
            {
                ViewBag.PrinterCount = StaticData.CountPrinters(item.Id).ToString();
            }
            return View(item);
        }

        //redirect to site
        public ActionResult GoToSite(string url)
        {
            return Redirect(url);
        }

        // GET: Items/Create
        // create item of a type (id) - first step
        // creates and passes viewmodel to the view
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // get item type by id
            var itemType = await _db.ItemTypes.FindAsync(id);
            if (itemType == null)
                return HttpNotFound();
            // create main item viewmodel
            var itemModel = new ItemViewModel
            {
                ItemTypeId = (int)id,
                ItemTypeName = itemType.Name
            };
            // add collection of viewmodels containing attribute-value pairs to the main viewmodel
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
        // create item - second step
        // get complete viewmodel from view and create new model item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            // check quantities
            if (item.Quantity < 0)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Неправильное количество (" + item.Quantity + ")!");
                return View(item);
            }
            if (item.MinQuantity < 0)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Неправильное количество (" + item.MinQuantity + ")!");
                return View(item);
            }
            if (_db.Items.FirstOrDefault(i => i.Name == item.Name) != null)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Элемент с таким именем уже существует в базе (" + item.Name + ")!");
                return View(item);
            }
            // create new model item
            var newItem = new Item
            {
                Name = item.Name,
                Quantity = item.Quantity,
                MinQuantity = item.MinQuantity,
                ItemType = await _db.ItemTypes.FindAsync(item.ItemTypeId)
            };
            // add to db
            _db.Items.Add(newItem);
            // create new history item
            var newHistory = new History
            {
                Recieved = true,
                Quantity = item.Quantity,
                Date = DateTime.Now,
                Item = newItem
            };
            // add to db
            _db.Histories.Add(newHistory);
            // add history item to item history list
            newItem.Histories.Add(newHistory);
            // create attribute values from attribute viewmodels
            foreach (var attributeValue in item.AttributeValues)
            {
                var newValue = new ItemAttributeValue
                {
                    Attribute = await _db.ItemAttributes.FindAsync(attributeValue.AttributeId),
                    ParentItem = newItem,
                    Value = attributeValue.Value
                };
                // add value to created model item
                newItem.AttributeValues.Add(newValue);
                // add value to db
                _db.ItemAttributeValues.Add(newValue);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/Edit/5
        // edit item - first step
        // creates and passes viewmodel to the view
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // find item from model by id
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            // create viewmodel based on found item data
            var itemModel = new ItemViewModel
            {
                Id = (int) id,
                Name = item.Name,
                Quantity = item.Quantity,
                MinQuantity = item.MinQuantity,
                ItemTypeName = item.ItemType.Name,
                ItemTypeId = item.ItemType.Id
            };
            // fill attribute values collection of the viewmodel
            foreach (var attribute in item.ItemType.Attributes.OrderBy(a => a.Name))
            {
                // get attributes for the item type: if it exists in found item - add existing to viewmodel, if not - create new
                var existingAttributeValue = item.AttributeValues.FirstOrDefault(a => a.Attribute.Id == attribute.Id);
                if (existingAttributeValue == null)
                {
                    // add absent attribute
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
                    // add existing attribute value
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
        // edit item - second step
        // get complete viewmodel from view and modify model item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            // check quantities
            if (item.Quantity < 0)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Неправильное количество (" + item.Quantity + ")!");
                return View(item);
            }
            if (item.MinQuantity < 0)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Неправильное количество (" + item.MinQuantity + ")!");
                return View(item);
            }
            // find model item to edit
            var editItem = await _db.Items.FindAsync(item.Id);
            if (editItem == null)
                return HttpNotFound();
            // update editable values
            editItem.Name = item.Name;
            editItem.Quantity = item.Quantity;
            editItem.MinQuantity = item.MinQuantity;
            // modify existing or create new attribute values
            foreach (var attributeValue in item.AttributeValues)
            {
                var editValue = await _db.ItemAttributeValues.FindAsync(attributeValue.Id);
                // new attribute-value pair
                if (editValue == null)
                {
                    var newAttributeValue = new ItemAttributeValue
                    {
                        Attribute = await _db.ItemAttributes.FindAsync(attributeValue.AttributeId),
                        ParentItem = editItem,
                        Value = attributeValue.Value
                    };
                    // add new attribute value to db
                    _db.ItemAttributeValues.Add(newAttributeValue);
                    // add new attribute value to the model item
                    editItem.AttributeValues.Add(newAttributeValue);
                }
                // existing attribute-value pair
                else
                {
                    // update attribute value
                    editValue.Value = attributeValue.Value;
                    _db.Entry(editValue).State = EntityState.Modified;
                }
            }
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/Recieve/5
        // new items arrived to the storage - first step
        // creates and passes viewmodel to the view
        public async Task<ActionResult> Recieve(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // find item in db
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            // create viewmodel based on item with necessary data
            var itemModel = new ItemViewModel
            {
                Id = (int)id,
                Name = item.Name,
                Quantity = 1
            };
            return View(itemModel);
        }

        // POST: Items/Recieve/5
        // new items arrived to the storage - second step
        // get viewmodel from view and modify model item quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Recieve(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            // find item in db
            var editItem = await _db.Items.FindAsync(item.Id);
            if (editItem == null)
                return HttpNotFound();
            // increase quantity
            editItem.Quantity += item.Quantity;
            // create new history item
            var newHistory = new History
            {
                Recieved = true,
                Date = DateTime.Now,
                Item = editItem,
                Quantity = item.Quantity
            };
            // add new history item to db
            _db.Histories.Add(newHistory);
            // add new history item to modified item history list
            editItem.Histories.Add(newHistory);
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/RecieveOne/5
        // trip notebooks comeback
        // increase notebook item quantity by 1
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
        // grant an item to user - first step
        // creates and passes viewmodel to the view
        public async Task<ActionResult> Grant(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // find item in db
            var item = await _db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();
            // create viewmodel based on item with necessary data
            var itemModel = new ItemViewModel
            {
                Id = (int)id,
                Name = item.Name,
                Quantity = 1
            };
            return View(itemModel);
        }

        // POST: Items/Grant/5
        // grant an item to user - second step
        // get viewmodel from view and modify model item quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Grant(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            // find item in db
            var editItem = await _db.Items.FindAsync(item.Id);
            if (editItem == null)
                return HttpNotFound();
            // check if we give more than we have
            if (editItem.Quantity < item.Quantity)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Нельзя выдать больше, чем есть в наличии (" + editItem.Quantity + ")!");
                return View(item);
            }
            // find the persons (who gave and who took) in db
            var whoGave = await _db.Persons.FindAsync(item.WhoGaveId);
            var whoTook = await _db.Persons.FindAsync(item.WhoTookId);
            if (whoGave == null || whoTook == null)
                return View(item);
            // decrease model item quantity
            editItem.Quantity -= item.Quantity;
            // create new history item
            var newHistory = new History
            {
                Recieved = false,
                Date = DateTime.Now,
                Item = editItem,
                Quantity = item.Quantity,
                WhoGave = whoGave,
                WhoTook = whoTook
            };
            // add new history item to db
            _db.Histories.Add(newHistory);
            // add new history item to modified item history list
            editItem.Histories.Add(newHistory);
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Items/Delete/5
        // delete item - first step
        // pass the item model to view
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
        // delete item - second step
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Items.FindAsync(id);
            // check if current user belongs to admin group
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
