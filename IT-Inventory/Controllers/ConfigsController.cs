using IT_Inventory.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IT_Inventory.Controllers
{
    //controller for computer configurations reports made by aida
    [Authorize(Roles = @"RIVS\IT-dep")]
    public class ConfigsController : Controller
    {
        // GET: Config
        public ActionResult Index()
        {
            var rootDir = new DirectoryInfo(@"\\rivs.org\it\ConfigReporting\ConfigReports");
            var hostDirs = rootDir.GetDirectories();
            var reports = (from dir in hostDirs
                where !dir.Name.Contains("SERVER-")
                let lastReportDir = dir.GetDirectories().OrderByDescending(d => d.CreationTime).FirstOrDefault()
                where lastReportDir != null
                let lastReportFile = lastReportDir.GetFiles().OrderByDescending(d => d.CreationTime).FirstOrDefault()
                where lastReportFile != null
                select new ReportViewModel
                {
                    CompName = dir.Name,
                    ReportDate = lastReportFile.LastWriteTime,
                    UserName = (@"RIVS\" + Path.GetFileNameWithoutExtension(lastReportFile.FullName)).GetUserName()
                }).ToList();

            Task.Run(() => StaticData.RefreshComputers());

            return View(reports);
        }

        // GET: Config/Details/5
        public async Task<ActionResult> Details(string compName)
        {
            var report = await Report.GetReportAsync(compName);
            if (report != null)
                return View(report);
            return RedirectToAction("Index");
        }
    }
}
