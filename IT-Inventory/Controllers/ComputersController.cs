﻿using System;
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
        public ActionResult Index(string depCode, int? personId, string searchSoft = "", int page = 1)
        {
            List<Computer> dbComputers;
            if (personId == null)
                dbComputers = depCode == null 
                    ? _db.Computers.OrderBy(c => c.ComputerName).ToList() 
                    : _db.Computers.AsEnumerable().Where(c => c.ComputerName.StartsWith(depCode + '-')).OrderBy(c => c.ComputerName).ToList();
            else
                dbComputers = _db.Computers.OrderBy(c => c.ComputerName).Where(c => c.Owner.Id == personId).ToList();

            if (searchSoft != "")
                dbComputers = dbComputers.Where(c => c.Software.IndexOf(searchSoft, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            var pager = new Pager(dbComputers.Count, page, 8);
            var computers = dbComputers.Select(comp => new ComputerViewModel
            {
                Id = comp.Id,
                ComputerName = comp.ComputerName,
                Cpu = comp.CpuInvent,
                Ram = comp.RamInvent,
                Hdd = comp.HddInvent,
                MotherBoard = comp.MotherBoardInvent,
                VideoAdapter = comp.VideoAdapterInvent,
                Monitor = comp.MonitorInvent,
                Owner = comp.Owner == null ? string.Empty : comp.Owner.ShortName,
                HasRequests = comp.SupportRequests.Count > 0,
                HasModifications = comp.HasModifications,
                UpdateDate = comp.UpdateDate?.ToString("g") ?? string.Empty
            });

            var computersViewModel = new ComputerIndexViewModel
            {
                Computers = computers.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                DepCodes = _db.Computers.AsEnumerable().Select(c => c.ComputerName.Split('-').First()).Distinct().OrderBy(c => c).ToArray(),
                DepCode = depCode,
                SearchSoft = searchSoft == "" ? null : searchSoft,
                PersonSearch = personId != null
            };
            return View(computersViewModel);
        }

        // GET: Computers/HistoryIndex/5
        // fixed computer configuration changes
        public async Task<ActionResult> HistoryIndex(int? id, int? days)
        {
            if (id == null && days == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            IEnumerable<ComputerHistoryItem> historyItems;

            if (id != null && days != null)
            {
                var comp = await _db.Computers.FindAsync(id);
                if (comp == null)
                    return HttpNotFound();
                ViewBag.CompName = comp.ComputerName;
                historyItems = _db.ComputerHistory.AsEnumerable()
                    .Where(h => h.HistoryComputer == comp && (DateTime.Now - h.HistoryUpdated) < new TimeSpan((int)days, 0, 0, 0));
            }
            else if (id != null)
            {
                var comp = await _db.Computers.FindAsync(id);
                if (comp == null)
                    return HttpNotFound();
                ViewBag.CompName = comp.ComputerName;
                historyItems = _db.ComputerHistory.AsEnumerable()
                    .Where(h => h.HistoryComputer == comp);
            }
            else
                historyItems = _db.ComputerHistory.AsEnumerable()
                    .Where(h => (DateTime.Now - h.HistoryUpdated) < new TimeSpan((int)days, 0, 0, 0));

            var historyModels = historyItems.OrderByDescending(h => h.HistoryUpdated)
                .Select(history => new ComputerHistoryViewModel
                {
                    Id = history.Id,
                    Cpu = history.HistoryCpu,
                    Ram = history.HistoryRam,
                    Hdd = history.HistoryHdd,
                    MotherBoard = history.HistoryMotherBoard,
                    VideoAdapter = history.HistoryVideoAdapter,
                    Monitor = history.HistoryMonitor,
                    UpdateDate = history.HistoryUpdated?.ToString("g") ?? string.Empty,
                    Changes = history.Changes,
                    OwnerName = history.HistoryComputerOwner == null ? string.Empty : history.HistoryComputerOwner.ShortName,
                    ComputerName = history.HistoryComputer == null ? string.Empty : history.HistoryComputer.ComputerName,
                    CompId = history.HistoryComputer?.Id ?? 0
                }).ToList();

            return View(historyModels);
        }

        // GET: Computers/Details/5
        // details of a computer from the last aida report
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
                Hdd = comp.Hdd,
                MotherBoard = comp.MotherBoard,
                VideoAdapter = comp.VideoAdapter,
                Monitor = comp.Monitor,
                LastReportDate = comp.LastReportDate?.ToString("d MMMM HH:mm") ?? comp.UpdateDate?.ToString("d MMMM HH:mm"),
                Owner = comp.Owner == null ? string.Empty : comp.Owner.FullName,
                Software = comp.Software.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None)
            };
            return View(compModel);
        }

        // GET: Computers/HistoryDetails/5
        // details of a computer configuration from history
        public async Task<ActionResult> HistoryDetails(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var historyItem = await _db.ComputerHistory.FindAsync(id);
            if (historyItem?.HistoryComputer == null)
                return HttpNotFound();

            var historyModel = new ComputerHistoryViewModel
            {
                CompId = historyItem.HistoryComputer.Id,
                ComputerName = historyItem.HistoryComputer.ComputerName,
                UpdateDate = historyItem.HistoryUpdated?.ToString("D") ?? string.Empty,
                OwnerName = historyItem.HistoryComputerOwner == null ? string.Empty : historyItem.HistoryComputerOwner.FullName,
                Cpu = historyItem.HistoryCpu,
                Ram = historyItem.HistoryRam,
                Hdd = historyItem.HistoryHdd,
                MotherBoard = historyItem.HistoryMotherBoard,
                VideoAdapter = historyItem.HistoryVideoAdapter,
                Monitor = historyItem.HistoryMonitor,
                Software = historyItem.HistorySoftware.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None),
                Changes = historyItem.Changes,
                InstalledSoftware = historyItem.SoftwareInstalled?.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None),
                RemovedSoftware = historyItem.SoftwareRemoved?.Split(new[] { "[NEW_LINE]" }, StringSplitOptions.None)
            };
            return View(historyModel);
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
                Hdd = comp.HddInvent,
                MotherBoard = comp.MotherBoardInvent,
                VideoAdapter = comp.VideoAdapterInvent,
                Monitor = comp.MonitorInvent,
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
            comp.HddInvent = compModel.Hdd;
            comp.MotherBoardInvent = compModel.MotherBoard;
            comp.VideoAdapterInvent = compModel.VideoAdapter;
            comp.MonitorInvent = compModel.Monitor;
            comp.UpdateDate = DateTime.Now;
            var softString = new StringBuilder();
            foreach (var item in compModel.Software)
                softString.Append(item + "[NEW_LINE]");
            comp.SoftwareInvent = softString.ToString();
            _db.Entry(comp).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> SyncConfig(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var comp = await _db.Computers.FindAsync(id);
            if (comp == null)
                return HttpNotFound();
            comp.UpdateDate = DateTime.Now;
            comp.CpuInvent = comp.Cpu;
            comp.RamInvent = comp.Ram;
            comp.HddInvent = comp.Hdd;
            comp.MotherBoardInvent = comp.MotherBoard;
            comp.VideoAdapterInvent = comp.VideoAdapter;
            comp.MonitorInvent = comp.Monitor;
            comp.SoftwareInvent = comp.Software;
            _db.Entry(comp).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Redirect(Request.UrlReferrer.ToString());
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
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(computer);
            }
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
