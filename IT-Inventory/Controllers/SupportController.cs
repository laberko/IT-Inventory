using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModels;
using ActionMailer.Net.Mvc4;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web.UI;

namespace IT_Inventory.Controllers
{
    public class SupportController : Controller
    {
        private readonly InventoryModel _db = new InventoryModel();

        // GET: SupportRequests
        [OutputCache(NoStore = true, Location = OutputCacheLocation.Client, Duration = 30)]
        public async Task<ActionResult> Index(int? state, int? searchUserId, int? searchCompId, int page = 1)
        {
            ViewBag.State = state;
            ViewBag.Page = page;
            ViewBag.SearchUserId = searchUserId;
            ViewBag.SearchCompId = searchCompId;

            if (!Request.IsAjaxRequest())
                return View();

            var model = new SupportIndexViewModel();
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = _db.Persons.FirstOrDefault(p => p.AccountName == accountName);
            if (person == null)
                return HttpNotFound();
            model.UserId = person.Id;

            IEnumerable<SupportRequest> requests;
            //requests of a computer
            if (searchCompId != null)
            {
                var comp = await _db.Computers.FindAsync(searchCompId);
                if (comp != null)
                {
                    requests = comp.SupportRequests
                        .OrderBy(r => r.State)
                        .ThenByDescending(r => r.CreationTime).AsEnumerable();
                    model.SearchString = comp.ComputerName;
                }
                else
                    return HttpNotFound();
            }
            //requests of a user
            else if (searchUserId != null)
            {
                var user = await _db.Persons.FindAsync(searchUserId);
                if (user != null)
                {
                    requests = user.SupportRequests.OrderBy(r => r.State)
                        .ThenByDescending(r => r.CreationTime).AsEnumerable();
                    model.SearchString = user.FullName;
                }
                else
                    return HttpNotFound();
            }
            //all requests
            else requests = _db.SupportRequests
                    .OrderBy(r => r.State)
                    .ThenByDescending(r => r.CreationTime).AsEnumerable();

            List<SupportRequest> requestList;
            
            //non-IT user - show only user's requests
            if (!User.IsInRole(@"RIVS\IT-Dep"))
            {
                model.IsItUser = false;
                //user's requests with a selected state or all requests
                requestList = state != null
                    ? requests.Where(r => r.From == person && r.State == state).ToList()
                    : requests.Where(r => r.From == person).ToList();
            }
            else
            {
                model.IsItUser = true;
                //IT user - show all users' requests with a selected state or all requests
                requestList = state != null
                    ? requests.Where(r => r.State == state).ToList()
                    : requests.ToList();
            }

            var pager = new Pager(requestList.Count, page);
            model.Pager = pager;
            model.SupportRequests = requestList.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList();
            return PartialView("IndexPartial", model);
        }

        public async Task<ActionResult> Modifications(int? compId)
        {
            if (compId == null || !User.IsInRole(@"RIVS\IT-Dep"))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var comp = await _db.Computers.FindAsync(compId);
            if (comp == null || !comp.HasModifications)
                return HttpNotFound();
            var requests = comp.SupportRequests.Where(r => r.FinishTime != null).AsEnumerable();
            return View(requests.Where(r => r.Modifications != null).OrderByDescending(r => r.FinishTime).ToList());
        }

        // GET: SupportRequests/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var supportRequest = await _db.SupportRequests.FindAsync(id);
            if (supportRequest?.From == null)
                return HttpNotFound();
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = await _db.Persons.FirstOrDefaultAsync(p => p.AccountName == accountName);
            if (supportRequest.From != person && !User.IsInRole(@"RIVS\IT-Dep"))
                return HttpNotFound();
            ViewBag.UserName = supportRequest.From.FullName;
            ViewBag.ItUserName = supportRequest.To == null ? string.Empty : supportRequest.To.FullName;
            ViewBag.ComputerName = supportRequest.FromComputer == null ? string.Empty : supportRequest.FromComputer.ComputerName;
            return View(supportRequest);
        }

        // GET: SupportRequests/Create
        public ActionResult Create()
        {
            return View(new SupportRequestViewModel
            {
                //one day urgency by default
                Urgency = 4
            });
        }

        // POST: SupportRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SupportRequestViewModel supportRequest)
        {
            if (!ModelState.IsValid)
                return View(supportRequest);
            if (string.IsNullOrEmpty(supportRequest.Text))
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Введите текст заявки!");
                return View(supportRequest);
            }
            var fromIt = false;
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = _db.Persons.FirstOrDefault(p => p.AccountName == accountName);
            if (person == null)
            {
                ModelState.AddModelError(string.Empty, "Ошибка авторизации!");
                return View(supportRequest);
            }
            Computer comp = null;
            SupportFile requestFile = null;

            //support request has attached file
            if (supportRequest.Upload != null && supportRequest.Upload.ContentLength > 0)
            {
                if (supportRequest.Upload.ContentLength > 10485760)
                {
                    ModelState.AddModelError(string.Empty, "Файл должен быть меньше 10 МБ!");
                    return View(supportRequest);
                }
                var fileName = Path.GetFileName(supportRequest.Upload.FileName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    var dir = Directory.CreateDirectory(Path.Combine(HostingEnvironment.MapPath(@"~/SupportFiles"), Guid.NewGuid().ToString()));
                    var filePath = Path.Combine(dir.FullName, fileName);
                    supportRequest.Upload.SaveAs(filePath);
                    requestFile = new SupportFile
                    {
                        Path = filePath
                    };
                    _db.SupportFiles.Add(requestFile);
                }
            }

            var newRequest = new SupportRequest
            {
                Text = supportRequest.Text,
                Urgency = supportRequest.Urgency,
                Category = supportRequest.Category,
                CreationTime = DateTime.Now,
                FinishTime = null,
                File = requestFile
            };

            //created by IT-user
            if (supportRequest.ToId != 0 && supportRequest.FromId != 0)
            {
                var ituser = await _db.Persons.FindAsync(supportRequest.ToId);
                var user = await _db.Persons.FindAsync(supportRequest.FromId);
                if (ituser == null || user == null)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден!");
                    return View(supportRequest);
                }
                comp = _db.Computers.Where(c => c.Owner.Id == user.Id).OrderByDescending(c => c.LastReportDate).FirstOrDefault();
                newRequest.To = ituser;
                newRequest.From = user;
                newRequest.State = 1;
                newRequest.StartTime = DateTime.Now;
                user.SupportRequests.Add(newRequest);
                _db.Entry(user).State = EntityState.Modified;
                fromIt = true;
            }

            //created by ordinary user
            else
            {
                var compName = StaticData.GetCompName(Request.UserHostName);
                if (!string.IsNullOrEmpty(compName))
                    comp = _db.Computers.FirstOrDefault(c => c.ComputerName == compName);
                newRequest.From = person;
                newRequest.State = 0;
                newRequest.StartTime = null;
                person.SupportRequests.Add(newRequest);
                _db.Entry(person).State = EntityState.Modified;

            }

            if (comp != null)
            {
                newRequest.FromComputer = comp;
                comp.SupportRequests.Add(newRequest);
                _db.Entry(comp).State = EntityState.Modified;
            }

            _db.SupportRequests.Add(newRequest);
            _db.SaveChanges();

            using (var mailer = new EmailController())
            {
                EmailResult mail;
                if (fromIt)
                {
                    //mail for selected IT user
                    mail = await mailer.EditByIt(newRequest.Id, User.Identity.Name.GetUserName());
                    mail?.Deliver();
                    //mail for user (job started)
                    mail = await mailer.Accepted(newRequest.Id);
                    mail?.Deliver();
                }
                else
                {
                    //mail for IT users
                    mail = await mailer.NewFromUser(newRequest.Id);
                    mail?.Deliver();
                }
            }

            return RedirectToAction("Index");
        }

        // GET: SupportRequests/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var supportRequest = await _db.SupportRequests.FindAsync(id);
            if (supportRequest == null)
                return HttpNotFound();
            var requestViewModel = new SupportRequestViewModel
            {
                Id = supportRequest.Id,
                Category = supportRequest.Category,
                State = supportRequest.State
            };
            //a user is editing his request
            if (!User.IsInRole(@"RIVS\IT-Dep"))
            {
                var login = User.Identity.Name;
                var accountName = login.Substring(5, login.Length - 5);
                var person = await _db.Persons.FirstOrDefaultAsync(p => p.AccountName == accountName);
                if (supportRequest.From != person)
                    return HttpNotFound();
                requestViewModel.EditByIt = false;
                requestViewModel.Text = supportRequest.Text;
                requestViewModel.Mark = supportRequest.Mark.ToString();
                requestViewModel.FeedBack = supportRequest.FeedBack;
                requestViewModel.Urgency = supportRequest.Urgency;
                return View("EditByUser", requestViewModel);
            }
            //support is editing a request
            requestViewModel.EditByIt = true;
            requestViewModel.Comment = supportRequest.Comment;
            requestViewModel.SoftwareInstalled = supportRequest.SoftwareInstalled;
            requestViewModel.SoftwareRemoved = supportRequest.SoftwareRemoved;
            requestViewModel.SoftwareRepaired = supportRequest.SoftwareRepaired;
            requestViewModel.SoftwareUpdated = supportRequest.SoftwareUpdated;
            requestViewModel.OtherActions = supportRequest.OtherActions;
            requestViewModel.ToId = supportRequest.To?.Id ?? 0;
            requestViewModel.From = supportRequest.From;
            return View("EditByIt", requestViewModel);
        }

        // POST: SupportRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SupportRequestViewModel requestViewModel)
        {
            var started = false;
            var itUserChanged = false;
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = await _db.Persons.FirstOrDefaultAsync(p => p.AccountName == accountName);
            if (person == null || !ModelState.IsValid)
            {
                // return view with error message
                if (person == null)
                    ModelState.AddModelError(string.Empty, "Ошибка авторизации!");
                return View(requestViewModel.EditByIt ? "EditByIt" : "EditByUser", requestViewModel);
            }
            var supportRequest = await _db.SupportRequests.FindAsync(requestViewModel.Id);
            if (supportRequest == null)
                return HttpNotFound();
            supportRequest.Category = requestViewModel.Category;

            //a file was attached
            if (requestViewModel.Upload != null && requestViewModel.Upload.ContentLength > 0)
            {
                if (requestViewModel.Upload.ContentLength > 10485760)
                {
                    ModelState.AddModelError(string.Empty, "Файл должен быть меньше 10 МБ!");
                    return View(requestViewModel.EditByIt ? "EditByIt" : "EditByUser", requestViewModel);
                }
                var fileName = Path.GetFileName(requestViewModel.Upload.FileName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    var dir = Directory.CreateDirectory(Path.Combine(HostingEnvironment.MapPath(@"~/SupportFiles"), Guid.NewGuid().ToString()));
                    var filePath = Path.Combine(dir.FullName, fileName);
                    requestViewModel.Upload.SaveAs(filePath);
                    var requestFile = new SupportFile
                    {
                        Path = filePath
                    };
                    _db.SupportFiles.Add(requestFile);
                    supportRequest.File = requestFile;
                }
            }

            //edit by user
            if (!requestViewModel.EditByIt)
            {
                int mark;
                supportRequest.Text = requestViewModel.Text;
                supportRequest.Mark = int.TryParse(requestViewModel.Mark, out mark) ? mark : 0;
                if (supportRequest.State == 2 && supportRequest.Mark == 0)
                {
                    requestViewModel.State = 2;
                    ModelState.AddModelError(string.Empty, "Пожалуйста, выберите оценку");
                    return View("EditByUser", requestViewModel);
                }
                supportRequest.FeedBack = requestViewModel.FeedBack;
                supportRequest.Urgency = requestViewModel.Urgency;
            }

            //edit by IT-user
            else
            {
                if (requestViewModel.ToId != 0)
                {
                    var itUser = await _db.Persons.FindAsync(requestViewModel.ToId);
                    if (itUser != null)
                    {
                        //new request assigned to an IT-user - start it
                        if (supportRequest.State == 0)
                        {
                            supportRequest.State = 1;
                            supportRequest.StartTime = DateTime.Now;
                            started = true;
                        }
                        else if (supportRequest.To != itUser)
                            itUserChanged = true;
                        supportRequest.To = itUser;
                    }
                }
                supportRequest.Comment = requestViewModel.Comment;
                supportRequest.SoftwareInstalled = requestViewModel.SoftwareInstalled;
                supportRequest.SoftwareRemoved = requestViewModel.SoftwareRemoved;
                supportRequest.SoftwareRepaired = requestViewModel.SoftwareRepaired;
                supportRequest.SoftwareUpdated = requestViewModel.SoftwareUpdated;
                //supportRequest.HardwareId = requestViewModel.HardwareId;
                //supportRequest.HardwareReplaced = requestViewModel.HardwareReplaced;
                supportRequest.OtherActions = requestViewModel.OtherActions;
            }

            _db.Entry(supportRequest).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            using (var mailer = new EmailController())
            {
                EmailResult mail = null;
                //edit by IT-user
                if (requestViewModel.EditByIt)
                {
                    //job started right now, IT-user selected
                    if (started)
                    {
                        //mail for user (job started)
                        mail = await mailer.Accepted(requestViewModel.Id);
                        mail?.Deliver();
                        //mail for selected IT user (request assigned)
                        mail = await mailer.EditByIt(requestViewModel.Id, User.Identity.Name.GetUserName());
                        mail?.Deliver();
                    }
                    //job in progress, IT-user changed by another IT-user
                    else if (itUserChanged)
                    {
                        //mail for selected IT user (request assigned)
                        mail = await mailer.EditByIt(requestViewModel.Id, User.Identity.Name.GetUserName());
                        mail?.Deliver();
                    }
                }
                //edit by user
                else
                {
                    switch (supportRequest.State)
                    {
                        //job in progress
                        case 1:
                            //mail for selected IT user (request changed)
                            mail = await mailer.EditByUser(requestViewModel.Id);
                            break;
                        //job is done, user added mark and feedback
                        case 2:
                            //mail for IT managers
                            mail = await mailer.Feedback(requestViewModel.Id);
                            break;
                    }
                    mail?.Deliver();
                }
            }

            return RedirectToAction("Index");
        }

        // GET: SupportRequests/AcceptRequest/5
        public async Task<ActionResult> AcceptRequest(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var supportRequest = await _db.SupportRequests.FindAsync(id);
            if (supportRequest == null)
                return HttpNotFound();
            //request is already accepted or iser is not in IT-dep
            if (supportRequest.State != 0 || !User.IsInRole(@"RIVS\IT-Dep"))
                return RedirectToAction("Index");
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = _db.Persons.FirstOrDefault(p => p.AccountName == accountName);
            if (person == null)
                return RedirectToAction("Index");
            supportRequest.State = 1;
            supportRequest.StartTime = DateTime.Now;
            supportRequest.To = person;
            _db.Entry(supportRequest).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            using (var mailer = new EmailController())
            {
                //mail for user (job started)
                var mail = await mailer.Accepted((int)id);
                mail?.Deliver();
            }

            return RedirectToAction("Index");
        }

        // GET: SupportRequests/FinishRequest/5
        public async Task<ActionResult> FinishRequest(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var supportRequest = await _db.SupportRequests.FindAsync(id);
            if (supportRequest == null)
                return HttpNotFound();
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var person = _db.Persons.FirstOrDefault(p => p.AccountName == accountName);
            //request was not accepted or was accepted by other user
            if (supportRequest.State != 1 || supportRequest.To != person)
                return RedirectToAction("Index");
            var requestViewModel = new SupportRequestViewModel
            {
                Id = supportRequest.Id,
                Comment = supportRequest.Comment,
                SoftwareInstalled = supportRequest.SoftwareInstalled,
                SoftwareRemoved = supportRequest.SoftwareRemoved,
                SoftwareRepaired = supportRequest.SoftwareRepaired,
                SoftwareUpdated = supportRequest.SoftwareUpdated,
                OtherActions = supportRequest.OtherActions,
                From = supportRequest.From
            };
            return View("Finish", requestViewModel);
        }

        // POST: SupportRequests/FinishRequest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FinishRequest(SupportRequestViewModel requestViewModel)
        {
            var login = User.Identity.Name;
            var accountName = login.Substring(5, login.Length - 5);
            var itPerson = await _db.Persons.FirstOrDefaultAsync(p => p.AccountName == accountName);
            if (!ModelState.IsValid || itPerson == null)
                return View("Finish", requestViewModel);
            if (!User.IsInRole(@"RIVS\IT-Dep"))
            {
                // return view with error message
                ModelState.AddModelError(string.Empty, "Ошибка авторизации!");
                return View("Finish", requestViewModel);
            }

            var supportRequest = await _db.SupportRequests.FindAsync(requestViewModel.Id);
            if (supportRequest == null)
                return HttpNotFound();

            int hardwareId;
            int.TryParse(requestViewModel.HardwareId, out hardwareId);

            supportRequest.Comment = requestViewModel.Comment;
            supportRequest.SoftwareInstalled = requestViewModel.SoftwareInstalled;
            supportRequest.SoftwareRemoved = requestViewModel.SoftwareRemoved;
            supportRequest.SoftwareRepaired = requestViewModel.SoftwareRepaired;
            supportRequest.SoftwareUpdated = requestViewModel.SoftwareUpdated;
            supportRequest.HardwareId = hardwareId;
            supportRequest.HardwareQuantity = requestViewModel.HardwareQuantity > 0 && hardwareId > 0 ? requestViewModel.HardwareQuantity : 0;
            supportRequest.OtherActions = requestViewModel.OtherActions;
            supportRequest.State = 2;
            supportRequest.FinishTime = DateTime.Now;
            
            //grant selected hardware
            if (hardwareId > 0 && requestViewModel.HardwareQuantity > 0)
            {
                // find item in db
                var editItem = await _db.Items.FindAsync(hardwareId);
                if (editItem == null)
                    return HttpNotFound();

                // check if we give more than we have
                if (editItem.Quantity < requestViewModel.HardwareQuantity)
                {
                    // return view with error message
                    ModelState.AddModelError(string.Empty, "Нельзя выдать больше, чем есть в наличии (" + editItem.Quantity + " шт.)!");
                    return View("Finish", requestViewModel);
                }

                // decrease item quantity
                editItem.Quantity -= requestViewModel.HardwareQuantity;

                // create new history item
                var newHistory = new History
                {
                    Recieved = false,
                    Date = DateTime.Now,
                    Item = editItem,
                    Quantity = requestViewModel.HardwareQuantity,
                    WhoGave = itPerson,
                    WhoTook = supportRequest.From
                };
                // add new history item to db
                _db.Histories.Add(newHistory);
                // add new history item to modified item history list
                editItem.Histories.Add(newHistory);
                _db.Entry(editItem).State = EntityState.Modified;
            }

            _db.Entry(supportRequest).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            using (var mailer = new EmailController())
            {
                //mail for user (job finished)
                var mail = await mailer.Finished(requestViewModel.Id);
                mail?.Deliver();
            }

            return RedirectToAction("Index");
        }

        // GET: SupportRequests/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var supportRequest = await _db.SupportRequests.FindAsync(id);
            if (supportRequest == null)
                return HttpNotFound();
            if (!User.IsInRole(@"RIVS\IT-Dep"))
                return RedirectToAction("Index");
            return View(supportRequest);
        }

        // POST: SupportRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var supportRequest = await _db.SupportRequests.FindAsync(id);
            if (supportRequest == null)
                return HttpNotFound();
            _db.SupportRequests.Remove(supportRequest);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public ActionResult DownloadFile(string filename)
        {
            return File(filename, "multipart/form-data", Path.GetFileName(filename));
        }

        public ActionResult DownloadImage(string filename)
        {
            return File(filename, "image/jpeg");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
