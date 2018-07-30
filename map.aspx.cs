using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _Map : Page
    {

        protected DataTable table;
        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        public string jsonCoordinates;
        public double mapXCoordinate;
        public double mapYCoordinate;
        protected Config config = DataAccess.GetConfig();
        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up

            try
            {
                int routeId = int.Parse(Request.QueryString["routeId"]);
                DataTable dtRoute = DataAccess.GetRouteDetailsData(routeId);
                
                if (!(dtRoute is null))
                {
                    List<Location> locations = DataAccess.ConvertRouteDetailsDataTableToLocationCoordinates(dtRoute);
                    List<Coordinates> coordinates = new List<Coordinates>();
                    foreach (Location location in locations)
                        coordinates.Add(location.coordinates);

                    jsonCoordinates += new JavaScriptSerializer().Serialize(coordinates);
                }

                if (jsonCoordinates is null)
                    jsonCoordinates = "\"\"";

                Calculation.origin = DataAccess.GetLocationById(config.Calculation.OriginLocationId);
                mapXCoordinate = Calculation.origin.coordinates.lat;
                mapYCoordinate = Calculation.origin.coordinates.lng;

                ClientScript.RegisterStartupScript(GetType(), "Javascript", "javascript:showMap(); ", true);
                ClientScript.RegisterStartupScript(GetType(), "Javascript", "javascript:addMarker();", true);

                //if (!Page.IsPostBack)
                //    BindListView();
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString());
            }
        }

        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {

        }

        /*protected void BindListView()
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
        */
    }
}