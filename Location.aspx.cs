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
using CsvHelper.Configuration;
using System.Text.RegularExpressions;
using CsvHelper.TypeConversion;

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
                PopulateDdlSearchFilter();
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

        private void PopulateDdlSearchFilter()
        {
            ListItem account = new ListItem();
            account.Value = "account";
            account.Text = "Location Name";
            ListItem address = new ListItem();
            address.Value = "address";
            address.Text = "Address";
            ListItem contactName = new ListItem();
            contactName.Value = "contact_name";
            contactName.Text = "Contact Name";
            ListItem contactEmail = new ListItem();
            contactEmail.Value = "contact_email";
            contactEmail.Text = "Contact Email";

            lstSearchFilters.Items.Insert(0, account);
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

        protected DropDownList PopulateLocationTypeDropDown(DropDownList ddlLocationType)
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
                string account = ((TextBox)LocationsListView.EditItem.FindControl("txtEditClientName")).Text;
                string oilPickupSchedule = ((TextBox)LocationsListView.EditItem.FindControl("txtEditoilPickupSchedule")).Text;
                string greaseTrapPreferredTimeStart = ((TextBox)LocationsListView.EditItem.FindControl("txtEditgreaseTrapPreferredTimeStart")).Text;
                string greaseTrapPreferredTimeEnd = ((TextBox)LocationsListView.EditItem.FindControl("txtEditgreaseTrapPreferredTimeEnd")).Text;
                string lastVisisted = ((TextBox)LocationsListView.EditItem.FindControl("txtEditLastVisited")).Text;
                string address = ((TextBox)LocationsListView.EditItem.FindControl("txtEditAddress")).Text;
                string oilTankSize = ((TextBox)LocationsListView.EditItem.FindControl("txtEditoilTankSize")).Text;
                string contactName = ((TextBox)LocationsListView.EditItem.FindControl("txtEditContactName")).Text;
                string contactEmail = ((TextBox)LocationsListView.EditItem.FindControl("txtEditContactEmail")).Text;
                string vehicleSize = ((TextBox)LocationsListView.EditItem.FindControl("txtEditVehicleSize")).Text;
                bool oilPickupCustomer = ((CheckBox)LocationsListView.EditItem.FindControl("chkoilPickupCustomer")).Checked;
                bool greaseTrapCustomer = ((CheckBox)LocationsListView.EditItem.FindControl("chkgreaseTrapCustomer")).Checked;

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
                if (oilPickupSchedule != null && oilPickupSchedule != "")
                {
                    cmd.Parameters.AddWithValue("p_oil_pickup_schedule", NpgsqlTypes.NpgsqlDbType.Integer, oilPickupSchedule.Trim());
                }
                if (greaseTrapPreferredTimeStart != null && greaseTrapPreferredTimeStart != "")
                {
                    cmd.Parameters.AddWithValue("p_grease_trap_preferred_time_start", NpgsqlTypes.NpgsqlDbType.Time, greaseTrapPreferredTimeStart.Trim());
                }
                if (greaseTrapPreferredTimeEnd != null && greaseTrapPreferredTimeEnd != "")
                {
                    cmd.Parameters.AddWithValue("p_grease_trap_preferred_time_end", NpgsqlTypes.NpgsqlDbType.Time, greaseTrapPreferredTimeEnd.Trim());
                }
                if (address != null && address != "")
                {
                    cmd.Parameters.AddWithValue("p_address", NpgsqlTypes.NpgsqlDbType.Varchar, address.Trim());
                    DataAccess.UpdateDistanceFromSource(DataAccess.GetLocationById(id));
                    DataAccess.UpdateGpsCoordinates(address, id);
                }
                if (account != null && account != "")
                {
                    cmd.Parameters.AddWithValue("p_account", NpgsqlTypes.NpgsqlDbType.Varchar, account.Trim());
                }
                if (oilTankSize != null && oilTankSize != "")
                {
                    cmd.Parameters.AddWithValue("p_oil_tank_size", NpgsqlTypes.NpgsqlDbType.Integer, oilTankSize.Trim());
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
                cmd.Parameters.AddWithValue("p_oil_pickup_customer", NpgsqlTypes.NpgsqlDbType.Boolean, oilPickupCustomer);
                cmd.Parameters.AddWithValue("p_grease_trap_customer", NpgsqlTypes.NpgsqlDbType.Boolean, greaseTrapCustomer);

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
            string account = ((TextBox)e.Item.FindControl("txtInsertClientName")).Text;
            string oilPickupSchedule = ((TextBox)e.Item.FindControl("txtInsertoilPickupSchedule")).Text;
            string greaseTrapPreferredTimeStart = ((TextBox)e.Item.FindControl("txtGreaseTrapPreferredTimeStart")).Text;
            string greaseTrapPreferredTimeEnd = ((TextBox)e.Item.FindControl("txtGreaseTrapPreferredTimeEnd")).Text;
            string lastVisisted = ((TextBox)e.Item.FindControl("txtInsertLastVisited")).Text;
            string address = ((TextBox)e.Item.FindControl("txtInsertAddress")).Text;
            string oilTankSize = ((TextBox)e.Item.FindControl("txtInsertoilTankSize")).Text;
            string contactName = ((TextBox)e.Item.FindControl("txtInsertContactName")).Text;
            string contactEmail = ((TextBox)e.Item.FindControl("txtInsertContactEmail")).Text;
            string vehicleSize = ((TextBox)e.Item.FindControl("txtInsertVehicleSize")).Text;
            bool oilPickupCustomer = ((CheckBox)e.Item.FindControl("chkoilPickupCustomer")).Checked;
            bool greaseTrapCustomer = ((CheckBox)e.Item.FindControl("chkgreaseTrapCustomer")).Checked;
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
                    if (oilPickupSchedule != null && oilPickupSchedule != "")
                    {
                        cmd.Parameters.AddWithValue("p_oil_pickup_schedule", NpgsqlTypes.NpgsqlDbType.Integer, oilPickupSchedule.Trim());
                    }
                    if (greaseTrapPreferredTimeStart != null && greaseTrapPreferredTimeStart != "")
                    {
                        cmd.Parameters.AddWithValue("grease_trap_preferred_time_start", NpgsqlTypes.NpgsqlDbType.Time, greaseTrapPreferredTimeStart.Trim());
                    }
                    if (greaseTrapPreferredTimeEnd != null && greaseTrapPreferredTimeEnd != "")
                    {
                        cmd.Parameters.AddWithValue("grease_trap_preferred_time_end", NpgsqlTypes.NpgsqlDbType.Time, greaseTrapPreferredTimeEnd.Trim());
                    }
                    if (address != null && address != "")
                    {
                        cmd.Parameters.AddWithValue("p_address", NpgsqlTypes.NpgsqlDbType.Varchar, address.Trim());
                        DataAccess.UpdateGpsCoordinates(address, id);
                    }
                    if (account != null && account != "")
                    {
                        cmd.Parameters.AddWithValue("p_account", NpgsqlTypes.NpgsqlDbType.Varchar, account.Trim());
                    }
                    if (oilTankSize != null && oilTankSize != "")
                    {
                        cmd.Parameters.AddWithValue("p_oil_tank_size", NpgsqlTypes.NpgsqlDbType.Integer, oilTankSize.Trim());
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
                    cmd.Parameters.AddWithValue("p_oil_pickup_customer", NpgsqlTypes.NpgsqlDbType.Boolean, oilPickupCustomer);
                    cmd.Parameters.AddWithValue("p_grease_trap_customer", NpgsqlTypes.NpgsqlDbType.Boolean, greaseTrapCustomer);

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
            LocationSort(sender, "id");
        }

        protected void SortByaccount_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "account");
        }

        protected void SortByDistanceFromDepot_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "distance_from_source");
        }

        protected void SortByAddress_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "address");
        }

        protected void SortByPickupInterval_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "oil_pickup_schedule");
        }

        protected void SortByLastVisited_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "last_visited");
        }

        protected void SortBygreaseTrapPreferredTimeStart_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "grease_trap_preferred_time_start");
        }

        protected void SortBygreaseTrapPreferredTimeEnd_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "grease_trap_preferred_time_end");
        }

        protected void SortByCapacity_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "oil_tank_size");
        }

        protected void SortByVehicleSize_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "vehicle_size");
        }

        protected void SortByContactName_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "contact_name");
        }

        protected void SortByContactEmail_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "contact_email");
        }

        protected void SortByType_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "type");
        }

        protected void SortByDaysUntilDue_Click(object sender, ImageClickEventArgs e)
        {
            LocationSort(sender, "days_until_due");
        }

        private void LocationSort(object sender, string sortProperty)
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
            //This applies to a filtered search.  In other cases, a default of account is passed in, or a column sort columnName is passed in
            if (String.IsNullOrEmpty(columnFilterName))
                columnFilterName = "account";
            if (String.IsNullOrEmpty(columnSortName))
                if (ViewState[viewStatePropertyLocation] == null)
                    columnSortName = "account";
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
            Extensions.RoundDataTable(dataTable, 2);
            LocationsListView.DataSource = dataTable;
            LocationsListView.ItemPlaceholderID = "itemPlaceHolder";
            LocationsListView.DataBind();
        }


        private class ConvertUsingClassMap : ClassMap<Location>
        {
            public ConvertUsingClassMap()
            {
                AutoMap();
                Map(m => m.Address).Name("AddressFullOneLine");
            }
        }

        private class CustomBooleanTypeConverterCache : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (text.ToLower() == "yes")
                    return true;

                else if (text.ToLower() == "no")
                    return false;

                else if (string.IsNullOrEmpty(text))
                    return false;
                try
                {
                    return bool.Parse(text);
                }
                catch
                {
                    Logger.Error(String.Format("Failed to parse Bool with input text: {0} on line {1} of CSV", text, row.Context.Row));
                }
                return text;
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value.ToString();
            }
        }

        private class CustomIntTypeConverterCache : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (!string.IsNullOrEmpty(text))
                    try
                    {
                        return int.Parse(text);
                    }
                    catch 
                    {
                        Logger.Error(String.Format("Failed to parse Int with input text: {0} on line {1} of CSV", text, row.Context.Row));
                    }

                return default(int);
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value.ToString();
            }
        }

        private class CustomDoubleTypeConverterCache : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (!string.IsNullOrEmpty(text))
                    try
                    {
                        return double.Parse(text);
                    }
                    catch
                    {
                        Logger.Error(String.Format("Failed to parse Double with input text: {0} on line {1} of CSV", text, row.Context.Row));
                    }

                return default(double);
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value.ToString();
            }
        }

        private class CustomDateTimeTypeConverterCache : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (!string.IsNullOrEmpty(text))
                    try
                    {
                        return DateTime.Parse(text);
                    }
                    catch
                    {
                        Logger.Error(String.Format("Failed to parse DateTime with input text: {0} on line {1} of CSV", text,row.Context.Row));
                    }

                return text;
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value.ToString();
            }
        }

        private class CustomTimeSpanTypeConverterCache : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {

                if (!string.IsNullOrEmpty(text))
                    try
                    {
                        return TimeSpan.Parse(text);
                    }
                    catch
                    {
                        Logger.Error(String.Format("Failed to parse TimeSpan with input text: {0} on line {1} of CSV", text, row.Context.Row));
                    }

                return text;
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value.ToString();
            }
        }

        protected void BtnImportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                Stream stream = fileUpload.FileContent;
                StreamReader reader = new StreamReader(stream);
                List<Location> locations = new List<Location>();
                using (CsvReader csv = new CsvReader(reader))
                {
                    csv.Configuration.TypeConverterCache.RemoveConverter<bool>();
                    csv.Configuration.TypeConverterCache.RemoveConverter<TimeSpan>();
                    csv.Configuration.TypeConverterCache.RemoveConverter<DateTime>();
                    csv.Configuration.TypeConverterCache.AddConverter<bool>(new CustomBooleanTypeConverterCache());
                    csv.Configuration.TypeConverterCache.AddConverter<int>(new CustomIntTypeConverterCache());
                    csv.Configuration.TypeConverterCache.AddConverter<double>(new CustomDoubleTypeConverterCache());
                    csv.Configuration.TypeConverterCache.AddConverter<TimeSpan>(new CustomTimeSpanTypeConverterCache());
                    csv.Configuration.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeTypeConverterCache());

                    //remove underscores from header names\
                    csv.Configuration.HeaderValidated = null;
                    csv.Configuration.MissingFieldFound = null;
                    csv.Configuration.PrepareHeaderForMatch = (header, index) => Regex.Replace(header, "_", string.Empty).ToLower();
                    csv.Configuration.RegisterClassMap<ConvertUsingClassMap>();
                    locations = csv.GetRecords<Location>().ToList();
                }
                /*
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route_location;"))
                    DataAccess.RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route;"))
                    DataAccess.RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM location;"))
                    DataAccess.RunSqlCommandText(cmd);
                */

                DataAccess.InsertLocations(locations);

                if (Config.Features.locationsJettingRemoveOnImport)
                {
                    DataAccess.DeleteLocationsWildCardSearch("jetting");
                    DataAccess.DeleteLocationsWildCardSearch("install");
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

        private string ConvertLinesToCSV(List<List<string>> fileLines, char delimiter = ',')
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