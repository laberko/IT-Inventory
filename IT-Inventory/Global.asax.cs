using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace IT_Inventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private Worker _worker;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            //const string logPath = "C:\\ProgramData\\Inventory\\";
            //const string logFile = "Error.log";
            //using (var outFile = new StreamWriter(Path.Combine(logPath, logFile), true))
            //{
            //    outFile.WriteLineAsync("Test!");
            //}



            if (_worker == null)
            {
                _worker = new Worker();
                Task.Run(() => _worker.StartJobsAsync());
            }
        }
    }
}
