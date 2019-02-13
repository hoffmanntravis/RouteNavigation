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
using NLog;
using System.Linq;
using CsvHelper;

namespace RouteNavigation
{
    public partial class _Locations : Page
    {
        private string viewStatePropertyLocation = "locationSortProperty";
        private string viewStatePropertySortOrder = "locationSortAscending";
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        DataTable dataTable = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make async calls that fail while the page is still starting up
            if (!Page.IsPostBack)
            {
                populateDdlSearchFilter();
                BindListView();
            }
        }

        private void ResetSortImageUrls(Control ctrl)
        {
            foreach (Control subCtrl in ctrl.Controls)
            {
                if (subCtrl is Image && subCtrl.ID.StartsWith("imgSort"))
                {
                    ((Image)subCtrl).ImageUrl = "~/images/up_arrow.svg";
                }
                if (subCtrl.HasControls())
                    ResetSortImageUrls(subCtrl);
            }
        }

        private void populateDdlSearchFilter()
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

        /*protected void LocationstListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            DropDownList ddlEdit = e.Item.FindControl("ddlEditLocationType") as DropDownList;
            if (ddlEdit != null)
            {
                ddlEdit = populateLocationTypeDropDown(ddlEdit);
                ddlEdit.SelectedValue = (e.Item.FindControl("lblEditLocationType") as Label).Text;
                ddlEdit.DataBind();
            }
        }
        */

        protected DropDownList populateLocationTypeDropDown(DropDownList ddlLocationType)
        {
            DataTable dt = DataAccess.GetLocationTypes();

            ddlLocationType.DataSource = dt;
            ddlLocationType.DataTextField = "type";
            ddlLocationType.DataValueField = "id";
            return ddlLocationType;
        }

        /* protected void LocationsListView_ItemCreated(object sender, ListViewItemEventArgs e)
          {
               if ((e.Item != null) && (e.Item.ItemType == ListViewItemType.InsertItem))
              {
                  DropDownList ddlLocationType;
                  ddlLocationType = e.Item.FindControl("ddlInsertLocationType") as DropDownList;
                  ddlLocationType = populateLocationTypeDropDown(ddlLocationType);
                  ddlLocationType.DataBind();
              }

          }
          */
        protected void LocationsListView_RowDeleting(object sender, ListViewDeleteEventArgs e)
        {

            int id = int.Parse(e.Keys["id"].ToString());
            DataAccess.DeleteRouteDependencies(id);
            BindListView();
        }

        protected void LocationsListView_RowEditing(object sender, ListViewEditEventArgs e)
        {

            LocationsListView.EditIndex = e.NewEditIndex;
            BindListView();
        }

        protected void LocationsListView_RowUpdating(object sender, ListViewUpdateEventArgs e)
        {
            try
            {
                //Finding the controls from Gridview for the row which is going to update  
                int id = int.Parse(e.Keys["id"].ToString());
                //string clientPriority = ((TextBox)LocationsListView.EditItem.FindControl("txtEditClientPriority")).Text;
                string locationName = ((TextBox)LocationsListView.EditItem.FindControl("txtEditClientName")).Text;
                string pickupIntervalDays = ((TextBox)LocationsListView.EditItem.FindControl("txtEditPickupIntervalDays")).Text;
                string insertPickupWindowStartTime = ((TextBox)LocationsListView.EditItem.FindControl("txtEditPickupWindowStartTime")).Text;
                string insertPickupWindowEndTime = ((TextBox)LocationsListView.EditItem.FindControl("txtEditPickupWindowEndTime")).Text;
                string lastVisisted = ((TextBox)LocationsListView.EditItem.FindControl("txtEditLastVisited")).Text;
                string address = ((TextBox)LocationsListView.EditItem.FindControl("txtEditAddress")).Text;
                string capacityGallons = ((TextBox)LocationsListView.EditItem.FindControl("txtEditCapacityGallons")).Text;
                string contactName = ((TextBox)LocationsListView.EditItem.FindControl("txtEditContactName")).Text;
                string contactEmail = ((TextBox)LocationsListView.EditItem.FindControl("txtEditContactEmail")).Text;
                string vehicleSize = ((TextBox)LocationsListView.EditItem.FindControl("txtEditVehicleSize")).Text;
                bool hasOil = ((CheckBox)LocationsListView.EditItem.FindControl("chkHasOil")).Checked;
                bool hasGrease = ((CheckBox)LocationsListView.EditItem.FindControl("chkHasGrease")).Checked;

                NpgsqlCommand cmd = new NpgsqlCommand("update_location");
                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                //if (clientPriority != null && clientPriority != "")
                //{
                //    cmd.Parameters.AddWithValue("p_client_priority", NpgsqlTypes.NpgsqlDbType.Integer, clientPriority.Trim());
                //}
                if (lastVisisted != null && lastVisisted != "")
                {
                    cmd.Parameters.AddWithValue("p_last_visited", NpgsqlTypes.NpgsqlDbType.Date, lastVisisted.Trim());
                }
                if (pickupIntervalDays != null && pickupIntervalDays != "")
                {
                    cmd.Parameters.AddWithValue("p_pickup_interval_days", NpgsqlTypes.NpgsqlDbType.Integer, pickupIntervalDays.Trim());
                }
                if (insertPickupWindowStartTime != null && insertPickupWindowStartTime != "")
                {
                    cmd.Parameters.AddWithValue("p_pickup_window_start_time", NpgsqlTypes.NpgsqlDbType.Time, insertPickupWindowStartTime.Trim());
                }
                if (insertPickupWindowEndTime != null && insertPickupWindowEndTime != "")
                {
                    cmd.Parameters.AddWithValue("p_pickup_window_end_time", NpgsqlTypes.NpgsqlDbType.Time, insertPickupWindowEndTime.Trim());
                }
                if (address != null && address != "")
                {
                    cmd.Parameters.AddWithValue("p_address", NpgsqlTypes.NpgsqlDbType.Varchar, address.Trim());
                    DataAccess.UpdateDistanceFromSource(DataAccess.GetLocationById(id));
                    DataAccess.UpdateGpsCoordinates(address, id);
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
                cmd.Parameters.AddWithValue("p_has_oil", NpgsqlTypes.NpgsqlDbType.Boolean, hasOil);
                cmd.Parameters.AddWithValue("p_has_grease", NpgsqlTypes.NpgsqlDbType.Boolean, hasGrease);

                DataAccess.RunStoredProcedure(cmd);

            }
            catch (Exception exception)
            {
                string ErrorDetails = "Input Data of Update was not valid.  Please verify data and try again." + "<br>" + exception.Message;
                Logger.Error(exception);
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = ErrorDetails;
                LocationsListView.EditIndex = -1;
            }
            LocationsListView.EditIndex = -1;
            BindListView();
        }

        protected void LocationsListView_RowCancelingEdit(object sender, ListViewCancelEventArgs e)
        {
            LocationsListView.EditIndex = -1;
            BindListView();
        }

        protected void LocationsListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            (LocationsListView.FindControl("locationDataPager") as DataPager).SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            BindListView(null, TxtSearchFilter.Text);
        }

        protected void FilterLocationsListView_Click(Object sender, EventArgs e)
        {
            DataPager DataPager = LocationsListView.FindControl("locationDataPager") as DataPager;
            DataPager.SetPageProperties(0, DataPager.PageSize, false);
            BindListView(lstSearchFilters.SelectedValue, TxtSearchFilter.Text);
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
                id = int.Parse(DataAccess.ReadStoredProcedureAsString(cmd));

            //string clientPriority = ((TextBox)e.Item.FindControl("txtInsertClientPriority")).Text;
            string locationName = ((TextBox)e.Item.FindControl("txtInsertClientName")).Text;
            string pickupIntervalDays = ((TextBox)e.Item.FindControl("txtInsertPickupIntervalDays")).Text;
            string insertPickupWindowStartTime = ((TextBox)e.Item.FindControl("txtInsertPickupWindowStartTime")).Text;
            string insertPickupWindowEndTime = ((TextBox)e.Item.FindControl("txtInsertPickupWindowEndTime")).Text;
            string lastVisisted = ((TextBox)e.Item.FindControl("txtInsertLastVisited")).Text;
            string address = ((TextBox)e.Item.FindControl("txtInsertAddress")).Text;
            string capacityGallons = ((TextBox)e.Item.FindControl("txtInsertCapacityGallons")).Text;
            string contactName = ((TextBox)e.Item.FindControl("txtInsertContactName")).Text;
            string contactEmail = ((TextBox)e.Item.FindControl("txtInsertContactEmail")).Text;
            string vehicleSize = ((TextBox)e.Item.FindControl("txtInsertVehicleSize")).Text;
            bool hasOil = ((CheckBox)e.Item.FindControl("chkHasOil")).Checked;
            bool hasGrease = ((CheckBox)e.Item.FindControl("chkHasGrease")).Checked;
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand("insert_location"))
                {

                    //if (clientPriority != null && clientPriority != "")
                    //{
                    //    cmd.Parameters.AddWithValue("p_client_priority", NpgsqlTypes.NpgsqlDbType.Integer, clientPriority.Trim());
                    //}
                    if (lastVisisted != null && lastVisisted != "")
                    {
                        cmd.Parameters.AddWithValue("p_last_visited", NpgsqlTypes.NpgsqlDbType.Date, lastVisisted.Trim());
                    }
                    if (pickupIntervalDays != null && pickupIntervalDays != "")
                    {
                        cmd.Parameters.AddWithValue("p_pickup_interval_days", NpgsqlTypes.NpgsqlDbType.Integer, pickupIntervalDays.Trim());
                    }
                    if (insertPickupWindowStartTime != null && insertPickupWindowStartTime != "")
                    {
                        cmd.Parameters.AddWithValue("pickup_window_start_time", NpgsqlTypes.NpgsqlDbType.Time, insertPickupWindowStartTime.Trim());
                    }
                    if (insertPickupWindowEndTime != null && insertPickupWindowEndTime != "")
                    {
                        cmd.Parameters.AddWithValue("pickup_window_end_time", NpgsqlTypes.NpgsqlDbType.Time, insertPickupWindowEndTime.Trim());
                    }
                    if (address != null && address != "")
                    {
                        cmd.Parameters.AddWithValue("p_address", NpgsqlTypes.NpgsqlDbType.Varchar, address.Trim());
                        DataAccess.UpdateGpsCoordinates(address, id);
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
                    cmd.Parameters.AddWithValue("p_has_oil", NpgsqlTypes.NpgsqlDbType.Boolean, hasOil);
                    cmd.Parameters.AddWithValue("p_has_grease", NpgsqlTypes.NpgsqlDbType.Boolean, hasGrease);

                    DataAccess.RunStoredProcedure(cmd);
                }

                try
                {
                    // We just inserted a new location so we should recalculate routes to accomodate for this
                    //calculator.calculateRoutes();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }

                DataAccess.RefreshApiCache();
            }
            catch (Exception exception)
            {
                string ErrorDetails = "Input Data of insert was not valid.  Please verify data and try again." + "<br>" + exception.Message;
                Logger.Error(exception);
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = ErrorDetails;
            }
            LocationsListView.EditIndex = -1;
            BindListView();
        }


        protected void SortByLocationId_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "id");
        }

        protected void SortByLocationName_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "location_name");
        }

        protected void SortByDistanceFromDepot_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "distance_from_source");
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

        protected void SortByPickupWindowStartTime_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "pickup_window_start_time");
        }

        protected void SortByPickupWindowEndTime_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "pickup_window_end_time");
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

        protected void SortByType_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "type");
        }

        protected void SortByDaysUntilDue_Click(object sender, ImageClickEventArgs e)
        {
            locationSort(sender, "days_until_due");
        }

        private void locationSort(object sender, string sortProperty)
        {
            ImageButton image = (ImageButton)sender;
            bool sortAsc = true;
            ResetSortImageUrls(Page);

            if (ViewState[viewStatePropertySortOrder] == null)
            {
                image.ImageUrl = "~/images/down_arrow.svg";
                sortAsc = false;
            }
            else
            {
                if (ViewState[viewStatePropertySortOrder].ToString() == Boolean.TrueString)
                {
                    image.ImageUrl = "~/images/down_arrow.svg";
                    sortAsc = false;
                }
                else if (ViewState[viewStatePropertySortOrder].ToString() == Boolean.FalseString)
                {
                    image.ImageUrl = "~/images/up_arrow.svg";
                    sortAsc = true;
                }
            }
            ViewState[viewStatePropertySortOrder] = sortAsc.ToString();
            ViewState[viewStatePropertyLocation] = sortProperty;

            BindListView(null, TxtSearchFilter.Text, sortProperty, sortAsc);

        }

        protected void BtnImportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                Stream stream = fileUpload.FileContent;
                StreamReader reader = new StreamReader(stream);
                char delimiter = ',';

                List<List<string>> fileLines = new List<List<string>>();
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    List<string> splitLine = line.Split(delimiter).ToList();
                    fileLines.Add(splitLine);
                }

                if (!(fileLines.Count > 0))
                {
                    dataValidation.IsValid = false;
                    dataValidation.ErrorMessage = "Upload File is blank.  Please select a file to upload before clicking upload.";
                    return;
                }

                string[] expectedHeaders = { "last_visited", "client_priority", "address", "location_name", "capacity_gallons", "coordinates_latitude", "coordinates_longitude", "days_until_due", "pickup_interval_days", "distance_from_source", "contact_name", "contact_email", "vehicle_size", "pickup_window_end_time", "pickup_window_start_time", "location_type", "intended_pickup_date", "has_oil", "has_grease" };

                fileLines = updateHeaderDataForPostgreImport(expectedHeaders, fileLines);

                string updatedCsvContent = convertLinesToCSV(fileLines);
                string updatedCsvHeader = String.Join(",", fileLines.First());

                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route_location;"))
                    DataAccess.RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route;"))
                    DataAccess.RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM location;"))
                    DataAccess.RunSqlCommandText(cmd);

                DataAccess.RunPostgreImport(String.Format("location ({0}) ", updatedCsvHeader), updatedCsvContent);
                if (Config.Features.locationsJettingRemoveOnImport)
                {
                    DataAccess.deleteLocationsWildCardSearch("jetting");
                    DataAccess.deleteLocationsWildCardSearch("install");
                }
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
                csvData = DataAccess.RunPostgreExport("location", csvData);
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
            DataAccess.RefreshApiCache(false);
            LocationsListView.EditIndex = -1;
            BindListView();
        }

        private void BindListView(string columnFilterName = null, string filterString = null, string columnSortName = null, bool ascending = true)
        {
            //This applies to a filtered search.  In other cases, a default of location_name is passed in, or a column sort columnName is passed in
            if (String.IsNullOrEmpty(columnFilterName))
                columnFilterName = "location_name";
            if (String.IsNullOrEmpty(columnSortName))
                if (ViewState[viewStatePropertyLocation] == null)
                    columnSortName = "location_name";
                else
                    columnSortName = ViewState[viewStatePropertyLocation].ToString();

            if (ViewState[viewStatePropertySortOrder] != null)
            {
                {
                    if (ViewState[viewStatePropertySortOrder].ToString() == Boolean.TrueString)
                        ascending = true;

                    else if (ViewState[viewStatePropertySortOrder].ToString() == Boolean.FalseString)
                        ascending = false;
                }
            }

            filterString = TxtSearchFilter.Text;
            DataAccess.GetLocationData(dataTable, columnFilterName, filterString, columnSortName, ascending);
            extensions.RoundDataTable(dataTable, 2);
            LocationsListView.DataSource = dataTable;
            LocationsListView.ItemPlaceholderID = "itemPlaceHolder";
            LocationsListView.DataBind();
        }


        private List<List<string>> updateHeaderDataForPostgreImport(string[] expectedHeaders, List<List<string>> fileLines, char delimiter = ',')
        {
            List<string> headers = fileLines.First();
            List<string> expectedHeadersList = new List<string>(expectedHeaders);
            List<int> indexesToRemove = new List<int>();

            if (headers.Contains("TrackingNumber"))
            {
                if (headers.IndexOf("AddressFullOneLine") != -1)
                    headers[headers.IndexOf("AddressFullOneLine")] = "address";
                if (headers.IndexOf("Account") != -1)
                    headers[headers.IndexOf("Account")] = "location_name";
                if (headers.IndexOf("OilPickup_Customer") != -1)
                    headers[headers.IndexOf("OilPickup_Customer")] = "has_oil";
                if (headers.IndexOf("GreaseTrap_Customer") != -1)
                    headers[headers.IndexOf("GreaseTrap_Customer")] = "has_grease";
            }

            //remove columns in reverse order so the array size doesn't shift to the left as we are indexing through
            for (int x = headers.Count() - 1; x >= 0; x--)
                if (!(expectedHeadersList.Contains(headers[x])))
                    for (int y = 0; y < fileLines.Count; y++)
                        fileLines[y].RemoveAt(x);
            /*
            for (int x = 0; x < headers.Count(); x++)
            {
                if ((headers[x] == "has_oil" || headers[x] == "has_grease"))
                    for (int y = 0; y < fileLines.Count; y++)
                         if (fileLines[y].IndexOf("has_oil") != -1 && fileLines[y][fileLines[y].IndexOf("has_oil")] == "")
                            fileLines[y][x] = "false";
            }*/
            return fileLines;
        }

        private string convertLinesToCSV(List<List<string>> fileLines, char delimiter = ',')
        {
            string updatedContent = null;
            foreach (List<string> line in fileLines)
            {
                updatedContent += String.Join(delimiter.ToString(), line);
                updatedContent += Environment.NewLine;
            }
            return updatedContent;
        }

    }
}