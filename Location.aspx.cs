using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;
using System.Data;
using System.IO;
using System.Text;
using RouteNavigation;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;

namespace RouteNavigation
{
    public partial class _Locations : Page
    {
        protected DataAccess dataAccess;
        DataTable dataTable = new DataTable();

        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make async calls that fail while the page is still starting up
            dataAccess = new DataAccess();
            if (!Page.IsPostBack)
            {
                populateDrpSearchFilter();
                BindGridView();
            }
        }

        protected void populateDrpSearchFilter()
        {
            ListItem locationName = new ListItem();
            locationName.Value = "location_name";
            locationName.Text = "Location Name";
            ListItem address = new ListItem();
            address.Value = "address";
            address.Text = "Address";
            ListItem contactName = new ListItem();
            contactName.Value = "contact_name";
            contactName.Text = "Contact Name";
            ListItem contactEmail = new ListItem();
            contactEmail.Value = "contact_email";
            contactEmail.Text = "Contact Email";

            lstSearchFilters.Items.Insert(0, locationName);
            lstSearchFilters.Items.Insert(1, address);
            lstSearchFilters.Items.Insert(2, contactName);
            lstSearchFilters.Items.Insert(3, contactEmail);
        }

        protected void LocationsListView_RowDeleting(object sender, ListViewDeleteEventArgs e)
        {
            int id = int.Parse(e.Keys["id"].ToString());
            dataAccess.DeleteRouteDependencies(id);
            //calculator.calculateRoutes();
            BindGridView();
        }
        protected void LocationsListView_RowEditing(object sender, ListViewEditEventArgs e)
        {
            LocationsListView.EditIndex = e.NewEditIndex;
            BindGridView();
        }

        protected void LocationsListView_RowUpdating(object sender, ListViewUpdateEventArgs e)
        {
            try
            {
                //Finding the controls from Gridview for the row which is going to update  
                string id = e.Keys["id"].ToString();
                string clientPriority = ((TextBox)LocationsListView.EditItem.FindControl("txtEditClientPriority")).Text;
                string locationName = ((TextBox)LocationsListView.EditItem.FindControl("txtEditClientName")).Text;
                string pickupIntervalDays = ((TextBox)LocationsListView.EditItem.FindControl("txtEditPickupIntervalDays")).Text;
                string lastVisisted = ((TextBox)LocationsListView.EditItem.FindControl("txtEditLastVisited")).Text;
                string address = ((TextBox)LocationsListView.EditItem.FindControl("txtEditAddress")).Text;
                string capacityGallons = ((TextBox)LocationsListView.EditItem.FindControl("txtEditCapacityGallons")).Text;
                string contactName = ((TextBox)LocationsListView.EditItem.FindControl("txtEditContactName")).Text;
                string contactEmail = ((TextBox)LocationsListView.EditItem.FindControl("txtEditContactEmail")).Text;
                string vehicleSize = ((TextBox)LocationsListView.EditItem.FindControl("txtEditVehicleSize")).Text;
                NpgsqlCommand cmd = new NpgsqlCommand("update_location");
                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                if (clientPriority != null && clientPriority != "")
                {
                    cmd.Parameters.AddWithValue("p_client_priority", NpgsqlTypes.NpgsqlDbType.Integer, clientPriority.Trim());
                }
                if (lastVisisted != null && lastVisisted != "")
                {
                    cmd.Parameters.AddWithValue("p_last_visited", NpgsqlTypes.NpgsqlDbType.Date, lastVisisted.Trim());
                }
                if (pickupIntervalDays != null && pickupIntervalDays != "")
                {
                    cmd.Parameters.AddWithValue("p_pickup_interval_days", NpgsqlTypes.NpgsqlDbType.Integer, pickupIntervalDays.Trim());
                }
                if (address != null && address != "")
                {
                    cmd.Parameters.AddWithValue("p_address", NpgsqlTypes.NpgsqlDbType.Varchar, address.Trim());
                }
                if (locationName != null && locationName != "")
                {
                    cmd.Parameters.AddWithValue("p_location_name", NpgsqlTypes.NpgsqlDbType.Varchar, locationName.Trim());
                }
                if (capacityGallons != null && capacityGallons != "")
                {
                    cmd.Parameters.AddWithValue("p_capacity_gallons", NpgsqlTypes.NpgsqlDbType.Integer, capacityGallons.Trim());
                }
                if (contactName != null && contactName != "")
                {
                    cmd.Parameters.AddWithValue("p_contact_name", NpgsqlTypes.NpgsqlDbType.Varchar, contactName.Trim());
                }
                if (contactEmail != null && contactEmail != "")
                {
                    cmd.Parameters.AddWithValue("p_contact_email", NpgsqlTypes.NpgsqlDbType.Varchar, contactEmail.Trim());
                }
                if (vehicleSize != null && vehicleSize != "")
                {
                    cmd.Parameters.AddWithValue("p_vehicle_size", NpgsqlTypes.NpgsqlDbType.Integer, vehicleSize.Trim());
                }

                dataAccess.RunStoredProcedure(cmd);
                dataAccess.UpdateMatrixWeight(int.Parse(id));
                LocationsListView.EditIndex = -1;
                BindGridView();
            }
            catch (Exception exception)
            {
                string ErrorDetails = "Input Data of Update was not valid.  Please verify data and try again." + "<br>" + exception.Message;
                Logging.Logger.LogMessage(exception.ToString());
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = ErrorDetails;
                LocationsListView.EditIndex = -1;
            }
        }

        protected void LocationsListView_RowCancelingEdit(object sender, ListViewCancelEventArgs e)
        {
            LocationsListView.EditIndex = -1;
            BindGridView();
        }

        protected void LocationsListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            (LocationsListView.FindControl("locationDataPager") as DataPager).SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            BindGridView("location_name", TxtSearchFilter.Text);
        }

        protected void FilterLocationsListView_Click(Object sender, EventArgs e)
        {
            DataPager DataPager = LocationsListView.FindControl("locationDataPager") as DataPager;
            DataPager.SetPageProperties(0, DataPager.PageSize, false);
            BindGridView("location_name", TxtSearchFilter.Text);
        }

        protected void TdClientPriority_click(object sender, ListViewCancelEventArgs e)
        {
            //write logic to update column sort
        }

        protected void LocationsListView_RowInsert(object sender, ListViewInsertEventArgs e)
        {
            //Finding the controls from Gridview for the row which is going to update  
            int id;
            using (NpgsqlCommand cmd = new NpgsqlCommand("select_next_location_id"))
                id = int.Parse(dataAccess.ReadStoredProcedureAsString(cmd));

            string clientPriority = ((TextBox)e.Item.FindControl("txtInsertClientPriority")).Text;
            string locationName = ((TextBox)e.Item.FindControl("txtInsertClientName")).Text;
            string pickupIntervalDays = ((TextBox)e.Item.FindControl("txtInsertPickupIntervalDays")).Text;
            string lastVisisted = ((TextBox)e.Item.FindControl("txtInsertLastVisited")).Text;
            string address = ((TextBox)e.Item.FindControl("txtInsertAddress")).Text;
            string capacityGallons = ((TextBox)e.Item.FindControl("txtInsertCapacityGallons")).Text;
            string contactName = ((TextBox)e.Item.FindControl("txtInsertContactName")).Text;
            string contactEmail = ((TextBox)e.Item.FindControl("txtInsertContactEmail")).Text;
            string vehicleSize = ((TextBox)e.Item.FindControl("txtInsertVehicleSize")).Text;
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand("insert_location"))
                {

                    if (clientPriority != null && clientPriority != "")
                    {
                        cmd.Parameters.AddWithValue("p_client_priority", NpgsqlTypes.NpgsqlDbType.Integer, clientPriority.Trim());
                    }
                    if (lastVisisted != null && lastVisisted != "")
                    {
                        cmd.Parameters.AddWithValue("p_last_visited", NpgsqlTypes.NpgsqlDbType.Date, lastVisisted.Trim());
                    }
                    if (pickupIntervalDays != null && pickupIntervalDays != "")
                    {
                        cmd.Parameters.AddWithValue("p_pickup_interval_days", NpgsqlTypes.NpgsqlDbType.Integer, pickupIntervalDays.Trim());
                    }
                    if (address != null && address != "")
                    {
                        cmd.Parameters.AddWithValue("p_address", NpgsqlTypes.NpgsqlDbType.Varchar, address.Trim());
                    }
                    if (locationName != null && locationName != "")
                    {
                        cmd.Parameters.AddWithValue("p_location_name", NpgsqlTypes.NpgsqlDbType.Varchar, locationName.Trim());
                    }
                    if (capacityGallons != null && capacityGallons != "")
                    {
                        cmd.Parameters.AddWithValue("p_capacity_gallons", NpgsqlTypes.NpgsqlDbType.Integer, capacityGallons.Trim());
                    }
                    if (contactName != null && contactName != "")
                    {
                        cmd.Parameters.AddWithValue("p_contact_name", NpgsqlTypes.NpgsqlDbType.Varchar, contactName.Trim());
                    }
                    if (contactEmail != null && contactEmail != "")
                    {
                        cmd.Parameters.AddWithValue("p_contact_email", NpgsqlTypes.NpgsqlDbType.Varchar, contactEmail.Trim());
                    }
                    if (vehicleSize != null && vehicleSize != "")
                    {
                        cmd.Parameters.AddWithValue("p_vehicle_size", NpgsqlTypes.NpgsqlDbType.Integer, vehicleSize.Trim());
                    }
                    dataAccess.RunStoredProcedure(cmd);
                }

                try
                {
                    // We just inserted a new location so we should recalculate routes to accomodate for this
                    //calculator.calculateRoutes();
                }
                catch (Exception exception)
                {
                    Logging.Logger.LogMessage(exception.ToString());
                }

                dataAccess.UpdateMatrixWeight(id);
                dataAccess.RefreshApiCache();
                LocationsListView.EditIndex = -1;
                BindGridView();
            }
            catch (Exception exception)
            {
                string ErrorDetails = "Input Data of insert was not valid.  Please verify data and try again." + "<br>" + exception.Message;
                Logging.Logger.LogMessage(exception.ToString());
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = ErrorDetails;
            }

        }


        protected void SortByLocationId_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "id");
        }

        protected void SortByLocationName_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "location_name");
        }

        protected void SortByClientPriority_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "client_priority");
        }

        protected void SortByAddress_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "address");
        }

        protected void SortByPickupInterval_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "pickup_interval_days");
        }

        protected void SortByLastVisited_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "last_visited");
        }

        protected void SortByCapacity_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "capacity_gallons");
        }

        protected void SortByVehicleSize_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "vehicle_size");
        }

        protected void SortByContactName_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "contact_name");
        }

        protected void SortByContactEmail_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "contact_email");
        }

        protected void SortByDaysUntilDue_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "days_until_due");
        }

        protected void SortByMatrixWeight_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "matrix_weight");
        }

        protected void locationSort(object sender, string sortProperty)
        {
            string viewStatePropertyName = "sortLocation" + sortProperty;

            ImageButton image = (ImageButton)sender;
            if (ViewState[viewStatePropertyName] == null)
            {
                ViewState[viewStatePropertyName] = false;
                BindGridView(sortProperty, null, false);
                image.ImageUrl = "~/images/down_arrow.svg";
            }
            else if (ViewState[viewStatePropertyName].ToString() == Boolean.TrueString)
            {
                ViewState[viewStatePropertyName] = false;
                BindGridView(sortProperty, null, false);

                image.ImageUrl = "~/images/down_arrow.svg";
            }
            else if (ViewState[viewStatePropertyName].ToString() == Boolean.FalseString)
            {
                ViewState[viewStatePropertyName] = true;
                BindGridView(sortProperty, null, true);
                image.ImageUrl = "~/images/up_arrow.svg";
            }
        }

        protected void BtnImportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                Stream stream = fileUpload.FileContent;
                StreamReader reader = new StreamReader(stream);
                string[] expectedHeaders = { "last_visited", "client_priority", "address", "location_name", "capacity_gallons", "coordinates_latitude", "coordinates_longitude", "days_until_due", "pickup_interval_days", "matrix_weight", "distance_from_source", "contact_name", "contact_email", "vehicle_size", "visit_time" };

                string Content = reader.ReadToEnd();

                if (Content == null || Content == "")
                {
                    dataValidation.IsValid = false;
                    dataValidation.ErrorMessage = "Upload File is blank.  Please select a file to upload before clicking upload.";
                    return;
                }

                Content = purifyCsvDataForPostgreImport(expectedHeaders, Content);

                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route_location;"))
                    dataAccess.RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route;"))
                    dataAccess.RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM location;"))
                    dataAccess.RunSqlCommandText(cmd);

                string expectedHeaderString = String.Join(",", expectedHeaders);

                dataAccess.RunPostgreImport(String.Format("location ({0}) ", expectedHeaderString), Content);
                BindGridView();
            }
            catch (Exception exception)
            {
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = "Error Loading CSV" + "<br>" + exception.Message;
                Logging.Logger.LogMessage(exception.ToString());
            }
        }

        protected void BtnExportCsv_Click(object sender, EventArgs e)
        {
            string csvData = "";
            try
            {
                csvData = dataAccess.RunPostgreExport("location", csvData);
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
            Response.AddHeader("content-disposition", "attachment; filename=" + "export_locations_" + dateTime + ".csv");
            Response.BufferOutput = true;
            Response.OutputStream.Write(Content, 0, Content.Length);
            Response.End();
        }

        protected void RefreshApiCache_Click(object sender, EventArgs e)
        {
            dataAccess.RefreshApiCache(false);
            LocationsListView.EditIndex = -1;
            BindGridView();
        }

        protected void BindGridView(string columnName = "location_name", string filterString = null, bool ascending = true)
        {
            //This applies to a filtered search.  In other cases, a default of location_name is passed in, or a column sort columnName is passed in
            if (columnName == null)
            columnName = lstSearchFilters.SelectedValue;
            filterString = TxtSearchFilter.Text;
            dataAccess.GetLocationData(dataTable, columnName, filterString, ascending);
            extensions.RoundDataTable(dataTable, 2);
            LocationsListView.DataSource = dataTable;
            LocationsListView.ItemPlaceholderID = "itemPlaceHolder";
            LocationsListView.DataBind();
        }

        protected string purifyCsvDataForPostgreImport(string[] expectedHeaders, string Content, char delimiter = ',')
        {
            StringReader strReader = new StringReader(Content);
            string headerLine = strReader.ReadLine();
            string[] headers = headerLine.Split(delimiter);

            List<string> expectedHeadersList = new List<string>(expectedHeaders);
            List<int> indexesToRemove = new List<int>();

            for (int x = 0; x < headers.Length; x++)
            {
                if (!(expectedHeadersList.Contains(headers[x])))
                {
                    indexesToRemove.Add(x);
                }
            }

            string updatedContent = null;
            string line;

            StringReader strReader2 = new StringReader(Content);
            while ((line = strReader2.ReadLine()) != null)
            {
                string[] row = line.Split(delimiter);

                foreach (int index in indexesToRemove)
                {
                    row[index] = null;
                }
                string lineToAdd = null;
                //add comas between columns of row
                bool lineHasData = false;
                for (int x = 0; x < row.Length - 1; x++)
                {

                    if (lineHasData == false)
                    {
                        if (row[x] != null && row[x] != "")
                        {
                            lineHasData = true;
                        }
                    }
                    if (row[x] != null)
                    {
                        lineToAdd += row[x] + ",";
                    }
                }

                //add last item in row without a comma after
                lineToAdd += row[row.Length - 1];
                lineToAdd += Environment.NewLine;

                if (lineHasData)
                {
                    updatedContent += lineToAdd;
                }
            }
            return updatedContent;
        }
    }
}