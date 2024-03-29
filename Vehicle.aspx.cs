﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;
using System.Data;
using System.IO;
using System.Text;
using NLog;

namespace RouteNavigation
{
    public partial class _Vehicle : Page
    {
        private  Logger Logger = LogManager.GetCurrentClassLogger();
        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up

            if (!Page.IsPostBack)
            {
                BindListView();
            }
        }

        protected void VehiclesListView_RowDeleting(object sender, ListViewDeleteEventArgs e)
        {
            string id = e.Keys["id"].ToString();
            NpgsqlCommand cmd = new NpgsqlCommand("delete_vehicle");
            cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
            DataAccess.RunStoredProcedure(cmd);
            BindListView();
        }
        protected void VehiclesListView_RowEditing(object sender, ListViewEditEventArgs e)
        {
            VehiclesListView.EditIndex = e.NewEditIndex;
            BindListView();
        }

        protected void VehiclesListView_RowUpdating(object sender, ListViewUpdateEventArgs e)
        {
            try
            {
                //Finding the controls from Gridview for the row which is going to update  
                string id = e.Keys["id"].ToString();
                string vehicleName = ((TextBox)VehiclesListView.EditItem.FindControl("txtEditName")).Text;
                string vehicleModel = ((TextBox)VehiclesListView.EditItem.FindControl("txtEditModel")).Text;
                string vehicleoilTankSize = ((TextBox)VehiclesListView.EditItem.FindControl("txtEditoilTankSize")).Text;
                string vehiclePhysicalSize = ((TextBox)VehiclesListView.EditItem.FindControl("txtEditPhysicalSize")).Text;
                string vehicleOperational = ((TextBox)VehiclesListView.EditItem.FindControl("txtEditOperational")).Text;

                NpgsqlCommand cmd = new NpgsqlCommand("update_vehicle");
                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                if (vehicleName != null && vehicleName != "")
                {
                    cmd.Parameters.AddWithValue("p_name", NpgsqlTypes.NpgsqlDbType.Varchar, vehicleName);
                }
                if (vehicleModel != null && vehicleModel != "")
                {
                    cmd.Parameters.AddWithValue("p_model", NpgsqlTypes.NpgsqlDbType.Varchar, vehicleModel);
                }
                if (vehicleoilTankSize != null && vehicleoilTankSize != "")
                {
                    cmd.Parameters.AddWithValue("p_oil_tank_size", NpgsqlTypes.NpgsqlDbType.Double, vehicleoilTankSize);
                }
                if (vehiclePhysicalSize != null && vehiclePhysicalSize != "")
                {
                    cmd.Parameters.AddWithValue("p_physical_size", NpgsqlTypes.NpgsqlDbType.Integer, vehiclePhysicalSize);
                }
                if (vehicleOperational != null && vehicleOperational != "")
                {
                    cmd.Parameters.AddWithValue("p_operational", NpgsqlTypes.NpgsqlDbType.Boolean, vehicleOperational);
                }


                DataAccess.RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                string ErrorDetails = "Input Data of Update was not valid.  Please verify data and try again." + "<br>" + exception.Message;
                Logger.Error(exception);
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = ErrorDetails;
                VehiclesListView.EditIndex = -1;
            }
            VehiclesListView.EditIndex = -1;
            BindListView();
        }

        protected void VehiclesListView_RowCancelingEdit(object sender, ListViewCancelEventArgs e)
        {
            VehiclesListView.EditIndex = -1;
            BindListView();
        }

        protected void VehiclesListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            VehiclesListView.EditIndex = -1;
            BindListView();
        }

        protected void VehiclesListView_RowInsert(object sender, ListViewInsertEventArgs e)
        {

            //Finding the controls from Gridview for the row which is going to update  
            string vehicleName = ((TextBox)e.Item.FindControl("txtInsertName")).Text;
            string vehicleModel = ((TextBox)e.Item.FindControl("txtInsertModel")).Text;
            string vehicleoilTankSize = ((TextBox)e.Item.FindControl("txtInsertoilTankSize")).Text;
            string vehicleOperational = ((TextBox)e.Item.FindControl("txtInsertOperational")).Text;
            string vehiclePhysicalSize = ((TextBox)e.Item.FindControl("txtInsertPhysicalSize")).Text;

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("insert_vehicle");
                if (vehicleName != null && vehicleName != "")
                {
                    cmd.Parameters.AddWithValue("p_name", NpgsqlTypes.NpgsqlDbType.Varchar, vehicleName);
                }
                if (vehicleModel != null && vehicleModel != "")
                {
                    cmd.Parameters.AddWithValue("p_model", NpgsqlTypes.NpgsqlDbType.Varchar, vehicleModel);
                }
                if (vehicleoilTankSize != null && vehicleoilTankSize != "")
                {
                    cmd.Parameters.AddWithValue("p_oil_tank_size", NpgsqlTypes.NpgsqlDbType.Double, vehicleoilTankSize);
                }
                if (vehicleOperational != null && vehicleOperational != "")
                {
                    cmd.Parameters.AddWithValue("p_operational", NpgsqlTypes.NpgsqlDbType.Boolean, vehicleOperational);
                }
                if (vehiclePhysicalSize != null && vehiclePhysicalSize != "")
                {
                    cmd.Parameters.AddWithValue("p_physical_size", NpgsqlTypes.NpgsqlDbType.Integer, vehiclePhysicalSize);
                }

                DataAccess.RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                string ErrorDetails = "Input Data of insert was not valid.  Please verify data and try again." + "<br>" + exception.Message;
                Logger.Error(exception);
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = ErrorDetails;
            }
            VehiclesListView.EditIndex = -1;
            BindListView();
        }

        protected void FilterVehiclesListView_Click(Object sender, EventArgs e)
        {
            BindListView("name", TxtSearchFilter.Text);
        }

        protected void BtnImportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                Stream stream = fileUpload.FileContent;
                StreamReader reader = new StreamReader(stream);
                string Content = reader.ReadToEnd();

                if (String.IsNullOrEmpty(Content))
                {
                    dataValidation.IsValid = false;
                    dataValidation.ErrorMessage = "Upload File is blank.  Please select a file to upload before clicking upload.";
                    return;
                }
                NpgsqlCommand cmd = new NpgsqlCommand("delete FROM vehicle;");
                DataAccess.RunSqlCommandText(cmd);
                DataAccess.RunPostgreImport("vehicle", Content);
                BindListView();
            }
            catch (Exception exception)
            {
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = "Error Loading CSV" + "<br>" + exception.Message;
                Logger.Error(exception);
            }
        }

        protected void BtnExportCsv_Click(object sender, EventArgs e)
        {
            string csvData = "";
            try
            {
                csvData = DataAccess.RunPostgreExport("vehicle", csvData);
            }
            catch (Exception exception)
            {
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = "Error Exporting CSV" + "<br>" + exception.Message;
                return;
            }
            DateTime dateTime = DateTime.Now;
            byte[] Content = Encoding.ASCII.GetBytes(csvData);
            Response.ContentType = "text/csv";
            Response.AddHeader("content-disposition", "attachment; filename=" + "export_vehicles_" + dateTime + ".csv");
            Response.BufferOutput = true;
            Response.OutputStream.Write(Content, 0, Content.Length);
            Response.End();
        }

        private void BindListView(string columnName = "name", string filterString = null)
        {
            DataTable table = DataAccess.VehicleData(columnName, filterString);
            VehiclesListView.DataSource = table;
            VehiclesListView.ItemPlaceholderID = "itemPlaceHolder";
            VehiclesListView.DataBind();
        }
    }
}