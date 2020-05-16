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
        private  Logger Logger = LogManager.GetCurrentClassLogger();
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        public string routesJson;
        public double mapXCoordinate;
        public double mapYCoordinate;
        public int routeCount;
        private Random random = new Random();
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //initialize objects in page load since they make a sync calls that fail while the page is still starting up
                DataAccess.PopulateConfig();

                DataTable dtRoute;
                int routeId;
                if (Request.QueryString["routeId"] == null)
                    dtRoute = DataAccess.RouteDetailsData(true);
                else
                {
                    routeId = int.Parse(Request.QueryString["routeId"]);
                    dtRoute = DataAccess.RouteDetailsData(routeId);
                }

                if (!(dtRoute is null))
                {
                    List<Location> locations = DataAccess.ConvertRouteDetailsDataTableToLocations(dtRoute);
                    routeCount = locations.GroupBy(l => l.RouteId).Count();

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
                    var locationsGroup = locations.GroupBy(l => l.RouteId);

                    int colorIndex = 0;
                    colors.Shuffle(random);
                    foreach (var location in locationsGroup)
                    {
                        Route r = new Route();
                        r.AllLocations.AddRange(location.ToList());
                        Color color = colors[colorIndex];
                        colorIndex++;
                        r.Color = color;
                        r.Id = r.AllLocations[0].RouteId;
                        routes.Add(r);
                    }


                    routesJson += new JavaScriptSerializer().Serialize(routes);
                }

                if (routesJson is null)
                    routesJson = "\"\"";

                Config.Calculation.origin = DataAccess.LocationById(Config.Calculation.origin.Id);
                if (Config.Calculation.origin.Coordinates.Lat is null || Config.Calculation.origin.Coordinates.Lng is null)
                {
                    Config.Calculation.origin.Coordinates.Lat = 39.03583319;
                    Config.Calculation.origin.Coordinates.Lng = -76.917329664;
                }

                mapXCoordinate = Config.Calculation.origin.Coordinates.Lat.Value;
                mapYCoordinate = Config.Calculation.origin.Coordinates.Lng.Value;

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