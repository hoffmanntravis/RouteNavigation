﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _Map : Page
    {
        protected DataAccess dataAccess;
        protected DataTable table;
        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up

            try
            {
                double x = 51.3;
                double y = .7;
                ClientScript.RegisterStartupScript(GetType(), "Javascript", "javascript:showMap(); ", true);
                ClientScript.RegisterStartupScript(GetType(), "Javascript", "javascript:addMarker();", true);

                //if (!Page.IsPostBack)
                //    BindGridView();
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString());
            }
        }

        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {

        }

        /*protected void BindGridView()
        {
            string queryStringId = Request.QueryString["routeId"];
            if (queryStringId != null && queryStringId != "")
            {
                int routeId = int.Parse(queryStringId);

                table = dataAccess.GetRouteDetailsData(routeId);
                RouteDetailsListView.DataSource = table;
                RouteDetailsListView.DataBind();
            }
        }
        */
    }
}