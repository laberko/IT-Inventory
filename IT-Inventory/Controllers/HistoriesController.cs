using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class HistoriesController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Histories/5     - history for all or an item
        public async Task<ActionResult> Index(int? id)
        {
            return id == null 
                ? View(await _db.Histories.OrderByDescending(h => h.Date).ToListAsync()) 
                : View(await _db.Histories.Where(h => h.Item.Id == id).OrderByDescending(h => h.Date).ToListAsync());
        }

        // GET: Histories/Recieve/X       - recieve history for all time or for X days
        public async Task<ActionResult> Recieve(int? days)
        {
            return days == null 
                ? View(await _db.Histories.Where(h => h.Recieved).OrderByDescending(h => h.Date).ToListAsync()) 
                : View(await _db.Histories.Where(h => h.Recieved && DbFunctions.DiffDays(h.Date, DateTime.Now) < days)
                    .OrderByDescending(h => h.Date).ToListAsync());
        }

        // GET: Histories/Grant/X       - grant history for all time or for X days
        public async Task<ActionResult> Grant(int? days)
        {
            return days == null 
                ? View(await _db.Histories.Where(h => !h.Recieved).OrderByDescending(h => h.Date).ToListAsync()) 
                : View(await _db.Histories.Where(h => !h.Recieved && DbFunctions.DiffDays(h.Date, DateTime.Now) < days)
                    .OrderByDescending(h => h.Date).ToListAsync());
        }


        // GET: Histories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var history = await _db.Histories.FindAsync(id);
            if (history == null)
                return HttpNotFound();
            return View(history);
        }

        // POST: Histories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var history = await _db.Histories.FindAsync(id);
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(history);
            }
            _db.Histories.Remove(history);
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
