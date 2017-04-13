using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModels;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class ComputersController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Computers
        public ActionResult Index(int? personId, int page = 1)
        {
            var dbComputers = personId == null 
                ? _db.Computers.OrderBy(c => c.ComputerName).ToList() 
                : _db.Computers.OrderBy(c => c.ComputerName).Where(c => c.Owner.Id == personId).ToList();
            var pager = new Pager(dbComputers.Count, page, 12);
            var computers = dbComputers.Select(comp => new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                Cpu = comp.Cpu,
                Ram = comp.Ram,
                MotherBoard = comp.MotherBoard,
                Owner = comp.Owner == null ? string.Empty : comp.Owner.ShortName,
                HasRequests = comp.SupportRequests.Count > 0
            });
            var computersViewModel = new ComputerIndexViewModel
            {
                Computers = computers.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };
            return View(computersViewModel);
        }

        // GET: Computers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var comp = await _db.Computers.FindAsync(id);
            if (comp == null)
                return HttpNotFound();
            var software = comp.Software.Split(new[] {"[NEW_LINE]"}, StringSplitOptions.None);
            var compModel = new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                Cpu = comp.Cpu,
                Ram = comp.Ram,
                MotherBoard = comp.MotherBoard,
                VideoAdapter = comp.VideoAdapter,
                Owner = comp.Owner == null ? string.Empty : comp.Owner.FullName,
                //OwnerId = comp.Owner?.Id ?? 0,
                Software = software
            };


            return View(compModel);
        }

        // GET: Config/AidaDetails/name
        public async Task<ActionResult> AidaDetails(string name)
        {
            var report = await Report.GetReportAsync(name);
            return report != null ? RedirectToAction("Details", "Configs", new { compName = name}) : RedirectToAction("Index");
        }


        // GET: Computers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var comp = await _db.Computers.FindAsync(id);
            if (comp == null)
                return HttpNotFound();
            var software = comp.Software.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None);

            var compModel = new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                OwnerId = comp.Owner?.Id ?? 0,
                Software = software
            };




            return View(compModel);
        }

        // POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ComputerViewModel compModel)
        {
            if (!ModelState.IsValid)
                return View(compModel);
            var comp = await _db.Computers.FindAsync(compModel.Id);
            if (comp == null)
                return HttpNotFound();
            comp.Owner = await _db.Persons.FindAsync(compModel.OwnerId);
            var softString = new StringBuilder();
            foreach (var item in compModel.Software)
                softString.Append(item + "[NEW_LINE]");
            comp.Software = softString.ToString();
            _db.Entry(comp).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Computers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var computer = await _db.Computers.FindAsync(id);
            if (computer == null)
                return HttpNotFound();
            return View(computer);
        }

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var computer = await _db.Computers.FindAsync(id);
            if (computer == null)
                return HttpNotFound();
            _db.Computers.Remove(computer);
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
