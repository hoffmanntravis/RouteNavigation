using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using static RouteNavigation.ColorManagement;

namespace RouteNavigation
{
    public partial class _Map : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        public string routesJson;
        public double mapXCoordinate;
        public double mapYCoordinate;
        public int routeCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up
            DataAccess.PopulateConfig();
            try
            {
                DataTable dtRoute;
                int routeId;
                if (Request.QueryString["routeId"] == null)
                    dtRoute = DataAccess.GetRouteDetailsData(true);
                else
                {
                    routeId = int.Parse(Request.QueryString["routeId"]);
                    dtRoute = DataAccess.GetRouteDetailsData(routeId);
                }

                if (!(dtRoute is null))
                {
                    List<Location> locations = DataAccess.ConvertRouteDetailsDataTableToLocations(dtRoute);
                    routeCount = locations.GroupBy(l => l.routeId).Count();
                    int colorIndex = 0;
                    List<Color> colors = new List<Color>();
                    if (routeCount == 1)
                    {
                        colors.Add(Color.Blue);
                    }
                    else
                    {
                        for (double i = 0; i < 1; i += (double)((double)1 / (double)routeCount))
                            colors.Add(HSL2RGB(i, 0.5, 0.5));
                    }

                    List<Route> routes = new List<Route>();
                    var locationsGroup = locations.GroupBy(l => l.routeId);
                    foreach (var location in locationsGroup)
                    {
                        Route r = new Route();
                        r.allLocations.AddRange(location.ToList());
                        Color color = colors[colorIndex];
                        colorIndex++;
                        r.color = color;
                        r.id = r.allLocations[0].routeId;
                        routes.Add(r);
                    }


                    routesJson += new JavaScriptSerializer().Serialize(routes);
                }

                if (routesJson is null)
                    routesJson = "\"\"";

                Config.Calculation.origin = DataAccess.GetLocationById(Config.Calculation.origin.id);
                mapXCoordinate = Config.Calculation.origin.coordinates.lat;
                mapYCoordinate = Config.Calculation.origin.coordinates.lng;

                ClientScript.RegisterStartupScript(GetType(), "Javascript", "javascript:showMap(); ", true);

                //if (!Page.IsPostBack)
                //    BindListView();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {

        }

        /*private void BindListView()
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