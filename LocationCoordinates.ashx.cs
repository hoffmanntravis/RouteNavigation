﻿using Npgsql;
using RouteNavigation;
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
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class Handler1 : IHttpHandler
    {
        
        private DataTable table;
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            string queryStringId = context.Request.QueryString["locationId"];
            string jsonResponse = null;
            if (queryStringId != null && queryStringId != "")
            {
                int routeId = int.Parse(queryStringId);

                table = DataAccess.LocationData(routeId);

                jsonResponse = DataTableToJSONWithJavaScriptSerializer(table);
            }

            context.Response.ContentType = "text/plain";
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string DataTableToJSONWithJavaScriptSerializer(DataTable table)
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




