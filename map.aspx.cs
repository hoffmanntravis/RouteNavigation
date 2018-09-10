﻿using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _Map : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        public string locationsJson;
        public double mapXCoordinate;
        public double mapYCoordinate;

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
                    locationsJson += new JavaScriptSerializer().Serialize(locations);
                }

                if (locationsJson is null)
                    locationsJson = "\"\"";

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