﻿using NLog;
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
        protected GeneticAlgorithm ga = new GeneticAlgorithm();
        protected RouteCalculator calc;
        protected DataTable dataTable;
        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        protected string btnCalculateRoutesInitialText;
        static Object calcLock = new Object();
        protected void Page_Load(object sender, EventArgs e)
        {
            btnCalculateRoutesInitialText = BtnCalculateRoutes.Text;
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up

            if (!Page.IsPostBack)
            {
                DataAccess.UpdateDbConfigWithApiStrings();
                BindListView();
            }
        }
        protected void RoutesListView_PagePropertiesChanging(object sender, EventArgs e)
        {

        }

        protected void BtnCalculateRoutes_Click(object sender, EventArgs e)
        {
            try
            {
                DataAccess.RefreshApiCache();
                if (Monitor.TryEnter(calcLock))
                {
                    try
                    {

                        ga.calculateBestRoutes();
                    }
                    finally
                    {
                        Monitor.Exit(calcLock);
                    }
                }
                else
                {
                    Exception exception = new Exception("Calculations are already running.  Please check the batch table and wait until the current calculations are completed, and then recalcualte");
                    throw exception;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
            }
            BindListView();
            BtnCalculateRoutes.Enabled = true;
            BtnCalculateRoutes.Text = btnCalculateRoutesInitialText;
        }

        protected void BtnExportCsv_Click(object sender, EventArgs e)
        {
            string csvData = "";

            int latestBatchId = DataAccess.GetLatestBatchId();

            try
            {
                csvData = DataAccess.RunPostgreExport("(select * from route_details where batch_id = " + latestBatchId + ")", csvData);
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
            dataTable = DataAccess.GetRouteInformationData();
            if (dataTable.Rows.Count != 0)
            {
                activityId.Text = "ActivityId: " + (from DataRow dr in dataTable.Rows select dr["activity_id"]).FirstOrDefault().ToString();
            }
            RoutesListView.DataSource = dataTable;
            extensions.RoundDataTable(dataTable, 2);
            RoutesListView.ItemPlaceholderID = "itemPlaceHolder";
            RoutesListView.DataBind();
        }
    }

}