using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _Routes : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static object syncLock = new object();
        private GeneticAlgorithm ga = new GeneticAlgorithm();
        private DataTable dataTable;
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        private static Object calcLock = new Object();
        private static string calculateRoutesText = "Calculate Routes";
        private static string calculateRoutesCancelText = "Cancel Calculations";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!(Monitor.TryEnter(calcLock)))
                    BtnCalculateRoutes.Text = calculateRoutesCancelText;
                else
                {
                    BtnCalculateRoutes.Text = calculateRoutesText;
                    Monitor.Exit(calcLock);
                }

                DataAccess.PopulateConfig();

                if (!Page.IsPostBack)
                {
                    DataAccess.UpdateDbConfigWithApiStrings();
                    BindListView();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
            }
}
        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {

        }

        protected void BtnCancelCalculation_Click(object sender, EventArgs e)
        {

        }

        protected void BtnCalculateRoutes_Click(object sender, EventArgs e)
        {
            try
            {
                var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
                DataAccess.RefreshApiCache();

                if (Monitor.TryEnter(calcLock))
                {
                    try
                    {
                        ga.CalculateBestRoutes();
                    }
                    finally
                    {
                        Monitor.Exit(calcLock);
                    }
                }
                else
                {
                    DataAccess.UpdateCancellationStatus();
                    while (!(Monitor.TryEnter(calcLock)))
                    {
                        Thread.Sleep(100);
                    }
                    Monitor.Exit(calcLock);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
            }
            BindListView();
            BtnCalculateRoutes.Text = calculateRoutesText;
        }

        protected void BtnExportCsv_Click(object sender, EventArgs e)
        {
            string csvData = "";

            int latestBatchId = DataAccess.GetLatestBatchId();

            try
            {
                csvData = DataAccess.RunPostgreExport("(select * from route_details where batch_id = " + latestBatchId + " order by route_id,insert_order)", csvData);
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

        protected void BindListView()
        {
            try
            {
                dataTable = DataAccess.GetRouteInformationData();
                if (dataTable.Rows.Count != 0)
                    activityId.Text = "ActivityId: " + (from DataRow dr in dataTable.Rows select dr["activity_id"]).FirstOrDefault().ToString();

                RoutesListView.DataSource = dataTable;
                Extensions.RoundDataTable(dataTable, 2);
                RoutesListView.ItemPlaceholderID = "itemPlaceHolder";
                RoutesListView.DataBind();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
            }

        }
    }
}

