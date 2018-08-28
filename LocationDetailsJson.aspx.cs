using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using NLog;

namespace RouteNavigation
{
    public partial class _LocationDetailsJson : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private DataTable table;
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

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
            string queryStringId = Request.QueryString["locationId"];
            if (queryStringId != null && queryStringId != "")
            {
                int routeId = int.Parse(queryStringId);

                table = DataAccess.GetLocationData(routeId);

                lblCoordinatesJson.Text = DataTableToJSONWithJavaScriptSerializer(table);
            }
        }

        protected string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();

                childRow.Add("coordinates_latitude", row["coordinates_latitude"]);
                childRow.Add("coordinates_longitude", row["coordinates_longitude"]);

                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }

    }
}