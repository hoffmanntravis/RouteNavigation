using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _Routes : Page
    {
        protected GeneticAlgorithm ga = new GeneticAlgorithm();
        protected DataAccess dataAccess;
        protected RouteCalculator calc;
        protected DataTable table;
        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up
            dataAccess = new DataAccess();
            calc = new RouteCalculator();
            if (!Page.IsPostBack)
            {
                dataAccess.UpdateDbConfigWithApiStrings();
                BindGridView();
            }
        }
        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {

        }

        protected void BtnCalculateRoutes_Click(object sender, EventArgs e)
        {
            try
            {
                BtnCalculateRoutes.Visible = false;
                BindGridView();
                ga.calculateBestRoutes();
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString());
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
            }
            BindGridView();
            BtnCalculateRoutes.Visible = true;
        }

        protected void BtnExportCsv_Click(object sender, EventArgs e)
        {
            string csvData = "";
            try
            {
                csvData = dataAccess.RunPostgreExport("(select * from route_details)", csvData);
            }
            catch (Exception exception)
            {
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = "Error Exporting CSV" + "<br>" + exception.Message;
                return;
            }
            DateTime dateTime = DateTime.Now;
            byte[] Content = Encoding.ASCII.GetBytes(csvData);
            Response.ContentType = "text/csv";
            Response.AddHeader("content-disposition", "attachment; filename=" + "export_route_location_details_" + dateTime + ".csv");
            Response.BufferOutput = true;
            Response.OutputStream.Write(Content, 0, Content.Length);
            Response.End();
        }

        protected void BindGridView()
        {
            table = dataAccess.GetRouteInformationData();
            RoutesListView.DataSource = table;
            RoutesListView.ItemPlaceholderID = "itemPlaceHolder";
            RoutesListView.DataBind();
        }
    }

}