using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _CalculationStatus : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Response.Write(status);
            DataAccess.IterationStatus iterationStatus = DataAccess.GetCalcStatus();
            string jsonStatus = Newtonsoft.Json.JsonConvert.SerializeObject(iterationStatus);
            Response.Write(jsonStatus);
            /*if (status == true)
            {
                Response.StatusCode = 200;
            }
            else
            {
                Response.StatusCode = 500;
            }*/
        }
    }
}