using System.Web.Mvc;

namespace IT_Inventory.Controllers
{
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}