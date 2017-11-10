using System.Collections.Generic;
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
    public class PrintersController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Printers
        public async Task<ActionResult> Index(int? officeId)
        {
            var printers = new List<Printer>();
            if (officeId == null)
            {
                printers = await _db.Printers.OrderBy(p => p.Name).ToListAsync();
                ViewBag.Office = "";
                ViewBag.Count = printers.Count.ToString();
                return View(printers);
            }
            var office = _db.Offices.FirstOrDefault(o => o.Id == officeId);
            if (office == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            foreach(var dep in _db.Departments.Where(d => d.Office.Id == office.Id))
                printers.AddRange(_db.Printers.Where(printer => printer.Department.Id == dep.Id));
            ViewBag.Office = office.Name;
            ViewBag.Count = printers.Count.ToString();
            return View(printers.OrderBy(p => p.Name));
        }

        // GET: Printers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var printer = await _db.Printers.FindAsync(id);
            if (printer == null)
                return HttpNotFound();
            return View(printer);
        }

        //redirect to printer admin page
        public ActionResult Manage(string ip)
        {
            return Redirect(@"http://" + ip);
        }

        // GET: Printers/Create
        public ActionResult Create()
        {
            return View(new PrinterViewModel());
        }

        // POST: Printers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PrinterViewModel printer)
        {
            if (!ModelState.IsValid)
                return View(printer);
            if (!string.IsNullOrEmpty(printer.Ip) && !StaticData.IsIp(printer.Ip))
            {
                ModelState.AddModelError(string.Empty, "Неправильный IP-адрес: " + printer.Ip + "!");
                return View(printer);
            }
            var newPrinter = new Printer
            {
                Name = printer.Name,
                Ip = printer.Ip,
                Place = printer.Place,
                Department = await _db.Departments.FindAsync(printer.DepartmentId)
            };
            _db.Printers.Add(newPrinter);
            foreach (var cartridge in printer.CartridgeIds
                .Where(c => c != null)
                .Distinct()
                .Select(cartridgeId => _db.Items.FirstOrDefault(c => c.Id == cartridgeId))
                .Where(cartridge => cartridge != null))
            {
                cartridge.Printers.Add(newPrinter);
                newPrinter.Cartridges.Add(cartridge);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Printers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var printer = await _db.Printers.FindAsync(id);
            if (printer == null)
                return HttpNotFound();
            var printerModel = new PrinterViewModel
            {
                Id = printer.Id,
                Name = printer.Name,
                Ip = printer.Ip,
                Place = printer.Place,
                DepartmentId = printer.Department.Id,
            };
            foreach (var cartridge in printer.Cartridges)
                printerModel.CartridgeIds.Add(cartridge.Id);
            return View(printerModel);
        }

        // POST: Printers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PrinterViewModel printer)
        {
            if (!ModelState.IsValid)
                return View(printer);
            if (!string.IsNullOrEmpty(printer.Ip) && !StaticData.IsIp(printer.Ip))
            {
                ModelState.AddModelError(string.Empty, "Неправильный IP-адрес: " + printer.Ip + "!");
                return View(printer);
            }
            var editItem = await _db.Printers.FindAsync(printer.Id);
            if (editItem == null)
                return HttpNotFound();
            editItem.Name = printer.Name;
            editItem.Ip = printer.Ip;
            editItem.Place = printer.Place;
            var department = await _db.Departments.FindAsync(printer.DepartmentId);
            if (department == null)
                return View(printer);
            editItem.Department = department;
            //if (department.Printers.FirstOrDefault(p => p.Id == printer.Id) == null)
            //{
            //    department.Printers.Add(editItem);
            //    _db.Entry(department).State = EntityState.Modified;
            //}
            foreach (var cartridge in printer.CartridgeIds
                .Select(cartridgeId => _db.Items.FirstOrDefault(c => c.Id == cartridgeId))
                .Where(cartridge => cartridge != null))
            {
                if (cartridge.Printers.FirstOrDefault(p => p.Id == printer.Id) == null)
                {
                    cartridge.Printers.Add(editItem);
                    _db.Entry(cartridge).State = EntityState.Modified;
                }
                if (editItem.Cartridges.FirstOrDefault(c => c.Id == cartridge.Id) == null)
                    editItem.Cartridges.Add(cartridge);
            }
            foreach (var cartridge in editItem.Cartridges.ToList()
                .Where(cartridge => printer.CartridgeIds.FirstOrDefault(c => c == cartridge.Id) == null))
            {
                editItem.Cartridges.Remove(cartridge);
                cartridge.Printers.Remove(cartridge.Printers.FirstOrDefault(p => p.Id == printer.Id));
            }
            _db.Entry(editItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Printers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var printer = await _db.Printers.FindAsync(id);
            if (printer == null)
                return HttpNotFound();
            return View(printer);
        }

        // POST: Printers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var printer = await _db.Printers.FindAsync(id);
            if (printer == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            //{
            //    ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
            //    return View(printer);
            //}
            _db.Printers.Remove(printer);
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
