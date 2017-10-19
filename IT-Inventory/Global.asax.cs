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

            if (_worker == null)
            {
                _worker = new Worker();
                Task.Run(() => _worker.StartJobsAsync());
            }
        }
    }
}
