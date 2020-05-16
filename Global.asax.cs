using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using NLog;


namespace RouteNavigation
{
    public class Global : HttpApplication
    {
        private  Logger Logger = LogManager.GetCurrentClassLogger();
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DataAccess.CleanupNullBatchCalcs();
            DataAccess.PopulateConfig();
            DataAccess.UpdateIteration(null, null);
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Server.ClearError();
            Logger.Info(ex);
        }
    }
}