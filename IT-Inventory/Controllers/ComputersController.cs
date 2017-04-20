using System;
using System.Collections.Generic;
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
        // manually edited computers data
        public ActionResult Index(string depCode, int? personId, int page = 1)
        {
            List<Computer> dbComputers;
            if (personId == null)
                dbComputers = depCode == null 
                    ? _db.Computers.OrderBy(c => c.ComputerName).ToList() 
                    : _db.Computers.AsEnumerable().Where(c => c.ComputerName.StartsWith(depCode + '-')).OrderBy(c => c.ComputerName).ToList();
            else
                dbComputers = _db.Computers.OrderBy(c => c.ComputerName).Where(c => c.Owner.Id == personId).ToList();

            var pager = new Pager(dbComputers.Count, page, 11);
            var computers = dbComputers.Select(comp => new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                Cpu = comp.CpuInvent,
                Ram = comp.RamInvent,
                MotherBoard = comp.MotherBoardInvent,
                VideoAdapter = comp.VideoAdapterInvent,
                Owner = comp.Owner == null ? string.Empty : comp.Owner.ShortName,
                HasRequests = comp.SupportRequests.Count > 0,
                HasModifications = comp.HasModifications
            });
            var computersViewModel = new ComputerIndexViewModel
            {
                Computers = computers.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                DepCodes = _db.Computers.AsEnumerable().Select(c => c.ComputerName.Split('-').First()).Distinct().OrderBy(c => c).ToArray(),
                DepCode = depCode,
                PersonSearch = personId != null
        };
            return View(computersViewModel);
        }

        // GET: Computers/Details/5
        // details of a computer from aida report
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var comp = await _db.Computers.FindAsync(id);
            if (comp == null)
                return HttpNotFound();
            var compModel = new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                Cpu = comp.Cpu,
                Ram = comp.Ram,
                MotherBoard = comp.MotherBoard,
                VideoAdapter = comp.VideoAdapter,
                Owner = comp.Owner == null ? string.Empty : comp.Owner.FullName,
                Software = comp.Software.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None)
            };
            return View(compModel);
        }

        // GET: Config/AidaDetails/name
        // full aida report for a computer
        public async Task<ActionResult> AidaDetails(string name)
        {
            var report = await Report.GetReportAsync(name);
            return report != null ? RedirectToAction("Details", "Configs", new { compName = name}) : RedirectToAction("Index");
        }


        // GET: Computers/Edit/5
        // edit computer data
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var comp = await _db.Computers.FindAsync(id);
            if (comp == null)
                return HttpNotFound();
            var compModel = new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                OwnerId = comp.Owner?.Id ?? 0,
                Cpu = comp.CpuInvent,
                Ram = comp.RamInvent,
                MotherBoard = comp.MotherBoardInvent,
                VideoAdapter = comp.VideoAdapterInvent,
                Software = comp.SoftwareInvent.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None)
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
            comp.CpuInvent = compModel.Cpu;
            comp.RamInvent = compModel.Ram;
            comp.MotherBoardInvent = compModel.MotherBoard;
            comp.VideoAdapterInvent = compModel.VideoAdapter;
            var softString = new StringBuilder();
            foreach (var item in compModel.Software)
                softString.Append(item + "[NEW_LINE]");
            comp.SoftwareInvent = softString.ToString();
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
