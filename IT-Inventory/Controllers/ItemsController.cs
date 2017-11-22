using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class ItemsController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Items
        // list of all items (id==null) or items of specific type (id)
        public async Task<ActionResult> Index(int? id, int? officeId, int page = 1, bool urgent = false)
        {
            var model = new ItemIndexViewModel();
            switch (id)
            {
                // all items
                case null:
                    List<Item> items;
                    if (urgent == false)
                    {
                        switch (officeId)
                        {
                            case null:
                                items = await _db.Items.OrderBy(i => i.ItemType.Name).ThenBy(i => i.Name).ToListAsync();
                                break;
                            case 1:
                                items = await _db.Items.Where(i => i.Location == null || i.Location.Id == 1).OrderBy(i => i.ItemType.Name).ThenBy(i => i.Name).ToListAsync();
                                model.OfficeId = 1;
                                model.OfficeName = "Железноводская, 11";
                                break;
                            default:
                                items = await _db.Items.Where(i => i.Location.Id == officeId).OrderBy(i => i.ItemType.Name).ThenBy(i => i.Name).ToListAsync();
                                model.OfficeId = (int) officeId;
                                model.OfficeName = _db.Offices.FirstOrDefault(o => o.Id == officeId)?.Name;
                                break;
                        }
                    }
                    else
                        items = await _db.Items.Where(i => (i.Location == null || i.Location.Id == 1) && i.Quantity <= i.MinQuantity)
                                    .OrderBy(i => i.ItemType.Name).ThenBy(i => i.Name).ToListAsync();
                    var pager = new Pager(items.Count, page, 14);
                    model.Items = items.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize);
                    model.Pager = pager;
                    model.IsUrgent = urgent;
                    return View(model);
                // trip notebooks ordered by name
                case 8:
                    switch (officeId)
                    {
                        case null:
                            model.Items = await _db.Items.Where(i => i.ItemType.Id == id)
                                .OrderBy(i => i.AttributeValues.FirstOrDefault(a => a.Attribute.Id == 8).Value).ToListAsync();
                            break;
                        case 1:
                            model.Items = await _db.Items.Where(i => i.ItemType.Id == id && (i.Location == null || i.Location.Id == 1))
                                .OrderBy(i => i.AttributeValues.FirstOrDefault(a => a.Attribute.Id == 8).Value).ToListAsync();
                            model.OfficeId = 1;
                            model.OfficeName = "Железноводская, 11";
                            break;
                        default:
                            model.Items = await _db.Items.Where(i => i.ItemType.Id == id && i.Location.Id == officeId)
                                .OrderBy(i => i.AttributeValues.FirstOrDefault(a => a.Attribute.Id == 8).Value).ToListAsync();
                            model.OfficeId = (int)officeId;
                            model.OfficeName = _db.Offices.FirstOrDefault(o => o.Id == officeId)?.Name;
                            break;
                    }
                    return View("IndexOfNotebook", model);
                // items of a type
                default:
                    switch (officeId)
                    {
                        case null:
                            model.Items = await _db.Items.Where(i => i.ItemType.Id == id).OrderBy(i => i.Name).ToListAsync();
                            break;
                        case 1:
                            model.Items = await _db.Items.Where(i => i.ItemType.Id == id &&(i.Location == null || i.Location.Id == 1)).OrderBy(i => i.Name).ToListAsync();
                            model.OfficeId = 1;
                            model.OfficeName = "Железноводская, 11";
                            break;
                        default:
                            model.Items = await _db.Items.Where(i => i.ItemType.Id == id && i.Location.Id == officeId).OrderBy(i => i.Name).ToListAsync();
                            model.OfficeId = (int)officeId;
                            model.OfficeName = _db.Offices.FirstOrDefault(o => o.Id == officeId)?.Name;
                            break;
                    }
                    model.Type = await _db.ItemTypes.FindAsync(id);
                    return View("IndexOfType", model);
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
                ViewBag.PrinterCount = StaticData.CountCartridgePrinters(item.Id).ToString();
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
                    Value = string.Empty,
                    IsNumber = attribute.IsNumber
                });
            return View(itemModel);
        }

        // POST: Items/Create
        // create item - second step
        // get complete viewmodel from view and create new model item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            // check quantities
            if (model.Quantity < 0)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Неправильное количество (" + model.Quantity + ")!");
                return View(model);
            }
            if (model.MinQuantity < -1)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Неправильное количество (" + model.MinQuantity + ")!");
                return View(model);
            }

            // create new model item
            // if trip notebook:
            if (model.ItemTypeId == 8)
            {
                model.Quantity = 1;
                model.MinQuantity = 0;
            }
            var newItem = new Item
            {
                Name = model.Name,
                Quantity = model.Quantity,
                MinQuantity = model.MinQuantity,
                ItemType = await _db.ItemTypes.FindAsync(model.ItemTypeId),
                Location = await _db.Offices.FindAsync(model.TargetOfficeId)
            };
            // add to db
            _db.Items.Add(newItem);
            // create new history item
            var newHistory = new History
            {
                Recieved = true,
                Quantity = model.Quantity,
                Date = DateTime.Now,
                Item = newItem
            };
            // add to db
            _db.Histories.Add(newHistory);
            // add history item to item history list
            newItem.Histories.Add(newHistory);
            // create attribute values from attribute viewmodels
            foreach (var attributeValue in model.AttributeValues)
            {
                if (attributeValue.IsNumber && !StaticData.IsNumber(attributeValue.Value))
                {
                    // return view with error message
                    ModelState.AddModelError(string.Empty, attributeValue.Name +  " не может быть " + attributeValue.Value + "!");
                    return View(model);
                }
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
                        AttributeId = attribute.Id,
                        IsNumber = attribute.IsNumber
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
                        AttributeId = attribute.Id,
                        IsNumber = attribute.IsNumber
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
            if (item.MinQuantity < -1)
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
                if (attributeValue.IsNumber && !StaticData.IsNumber(attributeValue.Value))
                {
                    // return view with error message
                    ModelState.AddModelError(string.Empty, attributeValue.Name + " не может быть " + attributeValue.Value + "!");
                    return View(item);
                }
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
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = _db.Persons.FirstOrDefault(p => p.AccountName == accountName);
            if (person == null)
                return RedirectToAction("Index");
            // increase quantity
            editItem.Quantity += item.Quantity;
            // create new history item
            var newHistory = new History
            {
                Recieved = true,
                Date = DateTime.Now,
                Item = editItem,
                Quantity = item.Quantity,
                WhoTook = person
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
        public async Task<ActionResult> RecieveOneNotebook(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = _db.Items.Find(id);
            if (item == null)
                return HttpNotFound();
            item.Quantity++;
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = 8 } );
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
                Quantity = 1,
                ItemTypeId = item.ItemType.Id
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
            //if trip notebook
            if (item.ItemTypeId == 8)
                item.Quantity = 1;
            // check if we give more than we have
            if (editItem.Quantity < item.Quantity)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Нельзя выдать больше, чем есть в наличии (" + editItem.Quantity + " шт.)!");
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

        // GET: Items/Transfer/5
        // transfer an item to another location - first step
        // creates and passes viewmodel to the view
        public async Task<ActionResult> Transfer(int? id)
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
                Quantity = 1,
                ItemTypeId = item.ItemType.Id,
                SourceOfficeId = item.Location?.Id ?? 1,
                SourceOfficeName = item.Location == null ? "Железноводская" : item.Location.Name
            };
            return View(itemModel);
        }

        // POST: Items/Transfer/5
        // transfer an item to another location - second step
        // get viewmodel from view and modify model item location
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Transfer(ItemViewModel item)
        {
            if (!ModelState.IsValid)
                return View(item);
            //find transferred item in db
            var itemInOldOffice = await _db.Items.FindAsync(item.Id);
            if (itemInOldOffice == null)
                return HttpNotFound();
            //if trip notebook
            if (item.ItemTypeId == 8)
                item.Quantity = 1;
            //check if we transfer more than we have
            if (itemInOldOffice.Quantity < item.Quantity)
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Нельзя переместить больше, чем есть в наличии (" + itemInOldOffice.Quantity + " шт.)!");
                return View(item);
            }

            //find item in target location
            Item itemInNewOffice;
            if (item.TargetOfficeId == 1)
                //for the main office location may be null
                itemInNewOffice = await _db.Items.Where(i => i.Name == item.Name && (i.Location == null || i.Location.Id == 1)).FirstOrDefaultAsync();
            else
                itemInNewOffice = await _db.Items.Where(i => i.Name == item.Name && i.Location.Id == item.TargetOfficeId).FirstOrDefaultAsync();

            //item doesn't exist in target location or it's a trip notebook - create new item
            if (item.ItemTypeId == 8 || itemInNewOffice == null)
            {
                itemInNewOffice = new Item
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    MinQuantity = itemInOldOffice.MinQuantity,
                    ItemType = await _db.ItemTypes.FindAsync(itemInOldOffice.ItemType.Id),
                    Location = await _db.Offices.FindAsync(item.TargetOfficeId)
                };
                _db.Items.Add(itemInNewOffice);
                await _db.SaveChangesAsync();
                //copy attribute-value pairs from source item
                foreach (var attr in itemInOldOffice.AttributeValues)
                {
                    var newAttrValuePair = new ItemAttributeValue
                    {
                        Attribute = await _db.ItemAttributes.FindAsync(attr.Attribute.Id),
                        ParentItem = itemInNewOffice,
                        Value = attr.Value
                    };
                    _db.ItemAttributeValues.Add(newAttrValuePair);
                    itemInNewOffice.AttributeValues.Add(newAttrValuePair);
                }
                //cartridge
                if (item.ItemTypeId == 11)
                    foreach (var printer in itemInOldOffice.Printers)
                        itemInNewOffice.Printers.Add(await _db.Printers.FindAsync(printer.Id));
            }
            else
                itemInNewOffice.Quantity += item.Quantity;

            // decrease source item quantity
            //if trip notebook - delete
            if (item.ItemTypeId == 8)
            {
                foreach (var attr in _db.ItemAttributeValues.Where(a => a.ParentItem.Id == item.Id))
                    _db.ItemAttributeValues.Remove(attr);
                _db.Items.Remove(itemInOldOffice);
            }
            else
            {
                itemInOldOffice.Quantity -= item.Quantity;
                _db.Entry(itemInOldOffice).State = EntityState.Modified;
            }

            _db.Entry(itemInNewOffice).State = EntityState.Modified;
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
            if (item == null)
                return RedirectToAction("Index");
            // check if current user belongs to admin group
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(item);
            }
            foreach (var attr in _db.ItemAttributeValues.Where(a => a.ParentItem.Id == id))
                _db.ItemAttributeValues.Remove(attr);
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<RedirectToRouteResult> SendUrgentItemsMail()
        {
            var mailer = new EmailController();
            var mail = mailer.UrgentItemsWarning();
            mail.DeliverAsync();
            var log = "Сообщение о нехватке оборудования на складе отправлено на адреса: " + mail.Mail.To;
            await log.WriteToLogAsync(EventLogEntryType.Information, "Mailer");
            return RedirectToAction("Index");
        }

        //get collection of hardware name-quantity strings of a category
        [HttpPost]
        public ActionResult GetItemsOfCategory(string category)
        {
            SelectList itemSelect = null;
            int catId;
            if (!string.IsNullOrEmpty(category) && int.TryParse(category, out catId))
            {
                var items = _db.Items.Where(i => i.ItemType.Id == catId && i.Quantity > 0).OrderBy(i => i.Name).AsEnumerable();
                itemSelect = new SelectList(items, "Id", "NameQuantity");
            }
            return Json(itemSelect, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
