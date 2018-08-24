using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _RouteDetails : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        protected DataTable table;
        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up
            try
            {
                if (!Page.IsPostBack)
                    BindListView();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {
        }

        protected void BindListView()
        {
            string queryStringId = Request.QueryString["routeId"];
            if (queryStringId != null && queryStringId != "")
            {
                int routeId = int.Parse(queryStringId);

                table = DataAccess.GetRouteDetailsData(routeId);
                RouteDetailsListView.DataSource = table;
                RouteDetailsListView.DataBind();
            }
        }

    }
}