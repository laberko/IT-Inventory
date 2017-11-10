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
    public class DepartmentsController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: Departments
        public async Task<ActionResult> Index()
        {
            var departments = await _db.Departments.ToListAsync();
            return View(departments.OrderBy(d => d.Name));
        }

        // GET: Departments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var department = await _db.Departments.FindAsync(id);
            if (department == null)
                return HttpNotFound();
            return View(department);
        }

        // GET: Departments/Create
        //public ActionResult Create()
        //{
        //    return View(new DepartmentViewModel());
        //}

        // POST: Departments/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(DepartmentViewModel department)
        //{
        //    if (!ModelState.IsValid)
        //        return View(department);
        //    //department with such name found in db
        //    if (_db.Departments.Any(d => d.Name == department.Name))
        //        return new HttpStatusCodeResult(HttpStatusCode.Conflict);
        //    var office = await _db.Offices.FindAsync(department.OfficeId);
        //    if (office == null)
        //        return HttpNotFound();
        //    var newDep = new Department
        //    {
        //        Name = department.Name,
        //        Office = office
        //    };
        //    _db.Departments.Add(newDep);
        //    if (office.Departments.All(d => d.Name != department.Name))
        //    {
        //        office.Departments.Add(newDep);
        //        _db.Entry(office).State = EntityState.Modified;
        //    }
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        // GET: Departments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var department = await _db.Departments.FindAsync(id);
            if (department == null)
                return HttpNotFound();
            return View(new DepartmentViewModel
            {
                DepId = (int) id,
                Name = department.Name,
                OfficeId = department.Office?.Id ?? 0
            });
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentViewModel department)
        {
            if (!ModelState.IsValid)
                return View(department);
            var editDep = await _db.Departments.FindAsync(department.DepId);
            if (editDep == null)
                return HttpNotFound();
            var editOffice = await _db.Offices.FindAsync(department.OfficeId);
            editDep.Name = department.Name;
            editDep.Office = editOffice;
            _db.Entry(editDep).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Departments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var department = await _db.Departments.FindAsync(id);
            if (department == null)
                return HttpNotFound();
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department == null)
                return HttpNotFound();
            if (!User.IsInRole(@"RIVS\InventoryAdmin"))
            {
                ModelState.AddModelError(string.Empty, "У Вас нет прав на удаление! Обратитесь к системному администратору!");
                return View(department);
            }
            foreach (var person in _db.Persons.Where(p => p.Dep.Id == id))
                person.Dep = null;
            _db.Departments.Remove(department);
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
