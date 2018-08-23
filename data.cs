using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace RouteNavigation
{
    public static class DataAccess
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        static string apiKey = System.Configuration.ConfigurationManager.AppSettings["googleApiKey"];
        static string mapsBaseUrl = System.Configuration.ConfigurationManager.AppSettings["googleDirectionsMapsUrl"];
        static string illegalCharactersString = System.Configuration.ConfigurationManager.AppSettings["googleApiIllegalCharacters"];
        static string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        public static void UpdateDbConfigWithApiStrings()
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                char[] illegalCharacters = illegalCharactersString.ToCharArray();
                NpgsqlCommand cmd = new NpgsqlCommand("upsert_config");
                cmd.Parameters.AddWithValue("p_google_directions_maps_url", NpgsqlTypes.NpgsqlDbType.Varchar, mapsBaseUrl);
                cmd.Parameters.AddWithValue("p_google_api_key", NpgsqlTypes.NpgsqlDbType.Varchar, apiKey);
                cmd.Parameters.AddWithValue("p_google_api_illegal_characters", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Char, illegalCharacters);
                connection.Open();
                RunStoredProcedure(cmd);
                connection.Close();
            }
        }

        public static void RefreshApiCache(bool fillEmptyOnly = true)
        {
            List<Location> locations;
            if (fillEmptyOnly)
                locations = GetLocations().Where(l => l.coordinates.lat is Double.NaN || l.coordinates.lng is Double.NaN).ToList();
            else
                locations = GetLocations();

            foreach (Location location in locations)
            {
                UpdateGpsCoordinates(location.address, location.id);
                //Google API calls will fail if called too rapidly
                Thread.Sleep(2000);
            }
        }

        public static int GetNextRouteId()
        {
            int id;
            using (NpgsqlCommand cmd = new NpgsqlCommand("select_next_route_id"))
                id = int.Parse(ReadStoredProcedureAsString(cmd));
            return id;
        }

        public static int GetNextRouteBatchId()
        {
            int id;
            using (NpgsqlCommand cmd = new NpgsqlCommand("select_next_route_batch_id"))
                id = int.Parse(ReadStoredProcedureAsString(cmd));
            return id;
        }

        public static int RunStoredProcedure(NpgsqlCommand cmd)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                cmd.Connection = connection;
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                int result = cmd.ExecuteNonQuery();
                connection.Close();
                return result;
            }
        }

        public static void UpdateRouteBatchMetadata(int id, int locationsIntakeCount, int locationsProcessedCount, int locationsOrphanedCount, double totalDistanceMiles, TimeSpan totalTime, double averageRouteDistanceMiles, double averageRouteDistanceStdDev)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("update_route_batch_metadata"))
            {
                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                cmd.Parameters.AddWithValue("p_locations_intake_count", NpgsqlTypes.NpgsqlDbType.Integer, locationsIntakeCount);
                cmd.Parameters.AddWithValue("p_locations_processed_count", NpgsqlTypes.NpgsqlDbType.Integer, locationsProcessedCount);
                cmd.Parameters.AddWithValue("p_locations_orphaned_count", NpgsqlTypes.NpgsqlDbType.Integer, locationsOrphanedCount);
                cmd.Parameters.AddWithValue("p_total_distance_miles", NpgsqlTypes.NpgsqlDbType.Double, totalDistanceMiles);
                cmd.Parameters.AddWithValue("p_total_time", NpgsqlTypes.NpgsqlDbType.Interval, totalTime);
                cmd.Parameters.AddWithValue("p_average_route_distance_miles", NpgsqlTypes.NpgsqlDbType.Double, averageRouteDistanceMiles);
                cmd.Parameters.AddWithValue("p_route_distance_std_dev", NpgsqlTypes.NpgsqlDbType.Double, averageRouteDistanceStdDev);
                RunStoredProcedure(cmd);
            }
        }



        public static void InsertRouteBatch()
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("insert_route_batch"))
            {
                RunStoredProcedure(cmd);
            }
        }

        public static void InsertRouteLocation(int locationId, int insertOrder, int routeId)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("insert_route_location"))
            {
                cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
                cmd.Parameters.AddWithValue("p_location_id", NpgsqlTypes.NpgsqlDbType.Integer, locationId);
                cmd.Parameters.AddWithValue("p_insert_order", NpgsqlTypes.NpgsqlDbType.Integer, insertOrder);
                RunStoredProcedure(cmd);
            }
        }

        public static int RunSqlCommandText(NpgsqlCommand cmd)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                connection.Open();
                int result = cmd.ExecuteNonQuery();
                connection.Close();
                return result;
            }
        }

        public static string RunPostgreExport(string tableName, string csvData)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                connection.Open();
                csvData = connection.BeginTextExport("COPY " + tableName + " TO STDOUT DELIMITER ',' CSV HEADER").ReadToEnd();
                connection.Close();
            }
            return csvData;
        }

        public static void RunPostgreImport(string tableName, string Content)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                connection.Open();
                using (var writer = connection.BeginTextImport("COPY " + tableName + " FROM STDIN DELIMITER ',' CSV HEADER"))
                {
                    writer.Write(Content);
                }
                connection.Close();
            }
        }

        public static bool ReadStoredProcedureIntoDataTable(NpgsqlCommand cmd, DataTable dataTable)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
            cmd.Connection = connection;
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();

            NpgsqlDataReader reader = cmd.ExecuteReader();

            dataTable.Load(reader);
            connection.Close();
            }
            return true;
        }

        public static string ReadStoredProcedureAsString(NpgsqlCommand cmd)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                cmd.Connection = connection;
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                string result = cmd.ExecuteScalar().ToString();
                connection.Close();
                return result;
            }
        }

        public static DataTable GetVehicleData(string columnName = "name", string filterString = null)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_vehicle_with_filter");

                cmd.Parameters.AddWithValue("p_column_name", NpgsqlTypes.NpgsqlDbType.Text, columnName);
                if (String.IsNullOrEmpty(filterString))
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, filterString);

                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
            return dataTable;
        }

        public static DataTable GetLocationData(DataTable dataTable, string filterColumnName = null, string filterString = null, string columnSortString=null, bool ascending = true)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_location_with_filter");

                if (String.IsNullOrEmpty(filterColumnName))
                    cmd.Parameters.AddWithValue("p_column_filter_string", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("p_column_filter_string", NpgsqlTypes.NpgsqlDbType.Text, filterColumnName);

                if (String.IsNullOrEmpty(filterString))
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, filterString);

                if (ascending == false)
                    cmd.Parameters.AddWithValue("p_ascending", NpgsqlTypes.NpgsqlDbType.Boolean, false);
                else
                    cmd.Parameters.AddWithValue("p_ascending", NpgsqlTypes.NpgsqlDbType.Boolean, DBNull.Value);

                if (String.IsNullOrEmpty(columnSortString))
                    cmd.Parameters.AddWithValue("p_column_sort_string", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("p_column_sort_string", NpgsqlTypes.NpgsqlDbType.Text, columnSortString);

                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logger.Error(new StackFrame(1).GetMethod().Name);
                Logger.Error(exception.ToString());
            }
            return dataTable;
        }

        public static List<Location> GetLocations(string filterColumnName = "location_name", string filterString = null)
        {
            DataTable dataTable = new DataTable();
            GetLocationData(dataTable, filterColumnName, filterString);
            List<Location> list = ConvertDataTableToLocationsList(dataTable);
            return list;
        }

        public static DataTable GetLocationTypes()
        {
            UpdateDaysUntilDue();
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_location_types");
                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
            return dataTable;
        }

            public static DataTable GetLocationData(int id)
        {
            UpdateDaysUntilDue();
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_location_by_id");

                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
            return dataTable;
        }

        public static DataTable GetConfigData()
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_config");
                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
            return dataTable;
        }

        public static DataTable GetFeaturesData()
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_features");
                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
            return dataTable;
        }

        public static DataTable GetRouteInformationData()
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_information");
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public static DataTable getRouteBatchData()
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_batch");
            ReadStoredProcedureIntoDataTable(cmd, dataTable);
            return dataTable;
        }

        public static DataTable GetRouteDetailsData(int routeId)
        {
            DataTable dataTable = new DataTable();

            NpgsqlCommand cmd = new NpgsqlCommand("select_route_details");
            cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public static DataTable GetRouteData(int id)
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_by_id");
            cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Integer, id);
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public static Config GetConfig()
        {
            DataTable configsDataTable = GetConfigData();
            DataTable featuresDataTable = GetFeaturesData();

            Config config = ConvertDataTablesToConfig(configsDataTable, featuresDataTable);
            return config;
        }

        public static Location GetLocationById(int id)
        {
            UpdateDaysUntilDue();
            DataTable dataTable = GetLocationData(id);

            List<Location> list = ConvertDataTableToLocationsList(dataTable);
            if (list.Count > 0)
                return list.First();
            else
                return null;
        }

        public static Config ConvertDataTablesToConfig(DataTable configs, DataTable features)
        {
            Config config = new Config();
            foreach (DataRow row in configs.Rows)
            {
                if (row["origin_location_id"] != DBNull.Value)
                    config.Calculation.OriginLocationId = int.Parse(row["origin_location_id"].ToString());
                if (row["minimum_days_until_pickup"] != DBNull.Value)
                    config.Calculation.currentFillLevelErrorMarginPercent = double.Parse(row["current_fill_level_error_margin"].ToString());
                if (row["oil_pickup_average_duration"] != DBNull.Value)
                    config.Calculation.oilPickupAverageDurationMinutes = TimeSpan.Parse(row["oil_pickup_average_duration"].ToString()).TotalMinutes;
                if (row["grease_pickup_average_duration"] != DBNull.Value)
                    config.Calculation.greasePickupAverageDurationMinutes = TimeSpan.Parse(row["grease_pickup_average_duration"].ToString()).TotalMinutes;
                if (row["matrix_priority_multiplier"] != DBNull.Value)
                    config.matrix.priorityMultiplier = double.Parse(row["matrix_priority_multiplier"].ToString());
                if (row["matrix_days_until_due_exponent"] != DBNull.Value)
                    config.matrix.daysUntilDueExponent = double.Parse(row["matrix_days_until_due_exponent"].ToString());
                if (row["matrix_overdue_multiplier"] != DBNull.Value)
                    config.matrix.overDueMultiplier = double.Parse(row["matrix_overdue_multiplier"].ToString());
                if (row["matrix_distance_from_source"] != DBNull.Value)
                    config.matrix.distanceFromSourceMultiplier = double.Parse(row["matrix_distance_from_source"].ToString());
                if (row["matrix_distance_from_source"] != DBNull.Value)
                    config.matrix.distanceFromSourceMultiplier = double.Parse(row["matrix_distance_from_source"].ToString());
                if (row["maximum_days_overdue"] != DBNull.Value)
                    config.Calculation.maximumDaysOverdue = int.Parse(row["maximum_days_overdue"].ToString());
                if (row["workday_start_time"] != DBNull.Value)
                    config.Calculation.workdayStartTime = DateTime.Parse(row["workday_start_time"].ToString());
                if (row["workday_end_time"] != DBNull.Value)
                    config.Calculation.workdayEndTime = DateTime.Parse(row["workday_end_time"].ToString());
                if (row["route_distance_max_miles"] != DBNull.Value)
                    config.Calculation.routeDistanceMaxMiles = int.Parse(row["route_distance_max_miles"].ToString());
            }

            foreach (DataRow row in features.Rows)
            {
                if (row["feature_name"] as string == "vehicle_fill_level")
                    config.Features.vehicleFillLevel = bool.Parse(row["enabled"].ToString());
                if (row["feature_name"] as string == "prioritize_nearest_location")
                    config.Features.prioritizeNearestLocation = bool.Parse(row["enabled"].ToString());
            }

            return config;
        }

        public static List<Location> ConvertDataTableToLocationsList(DataTable dataTable)
        {
            List<Location> locations = new List<Location>();
            foreach (DataRow row in dataTable.Rows)
            {
                Location location = new Location();
                if (row["id"] != DBNull.Value)
                    location.id = int.Parse(row["id"].ToString());
                if (row["last_visited"] != DBNull.Value)
                    location.lastVisited = DateTime.Parse(row["last_visited"].ToString());
                if (row["client_priority"] != DBNull.Value)
                    location.clientPriority = int.Parse(row["client_priority"].ToString());
                if (row["location_name"] != DBNull.Value)
                    location.locationName = row["location_name"].ToString();
                if (row["address"] != DBNull.Value)
                    location.address = row["address"].ToString();
                if (row["days_until_due"] != DBNull.Value)
                    location.daysUntilDue = double.Parse(row["days_until_due"].ToString());
                if (row["capacity_gallons"] != DBNull.Value)
                    location.capacityGallons = double.Parse(row["capacity_gallons"].ToString());
                if (row["vehicle_size"] != DBNull.Value)
                    location.vehicleSize = int.Parse(row["vehicle_size"].ToString());
                if (row["coordinates_latitude"] != DBNull.Value)
                    location.coordinates.lat = double.Parse(row["coordinates_latitude"].ToString());
                if (row["coordinates_longitude"] != DBNull.Value)
                    location.coordinates.lng = double.Parse(row["coordinates_longitude"].ToString());
                if (row["distance_from_source"] != DBNull.Value)
                    location.distanceFromDepot = double.Parse(row["distance_from_source"].ToString());
                if (row["pickup_interval_days"] != DBNull.Value)
                    location.pickupIntervalDays = int.Parse(row["pickup_interval_days"].ToString());
                if (row["pickup_window_start_time"] != DBNull.Value)
                    location.pickupWindowStartTime = DateTime.Parse(row["pickup_window_start_time"].ToString());
                if (row["pickup_window_end_time"] != DBNull.Value)
                    location.pickupWindowEndTime = DateTime.Parse(row["pickup_window_end_time"].ToString());
                if (row["matrix_weight"] != DBNull.Value)
                    location.matrixWeight = double.Parse(row["matrix_weight"].ToString());
                if (row["contact_name"] != DBNull.Value)
                    location.contactName = row["contact_name"].ToString();
                if (row["contact_email"] != DBNull.Value)
                    location.contactEmail = row["contact_email"].ToString();
                if (row["visit_time"] != DBNull.Value)
                    location.visitTime = TimeSpan.Parse(row["visit_time"].ToString());
                if (row["type_text"] != DBNull.Value)
                    location.type = row["type_text"].ToString();
                locations.Add(location);
            }
            return locations;
        }

        public static List<Location> ConvertRouteDetailsDataTableToLocationCoordinates(DataTable dataTable)
        {
            List<Location> locations = new List<Location>();
            foreach (DataRow row in dataTable.Rows)
            {
                Location location = new Location();
                if (row["coordinates_latitude"] != DBNull.Value)
                    location.coordinates.lat = double.Parse(row["coordinates_latitude"].ToString());
                if (row["coordinates_longitude"] != DBNull.Value)
                    location.coordinates.lng = double.Parse(row["coordinates_longitude"].ToString());
                locations.Add(location);
            }
            return locations;
        }

        public static List<Vehicle> GetVehicles(string columnName = "name", string filterString = null)
        {
            DataTable dataTable = GetVehicleData(columnName, filterString);
            List<Vehicle> vehicles = ConvertDataTableToVehiclesList(dataTable);
            return vehicles;
        }

        public static List<Vehicle> ConvertDataTableToVehiclesList(DataTable dataTable)
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            foreach (DataRow row in dataTable.Rows)
            {
                Vehicle vehicle = new Vehicle();
                if (row["id"] != DBNull.Value)
                    vehicle.id = int.Parse(row["id"].ToString());
                if (row["capacity_gallons"] != DBNull.Value)
                    vehicle.capacityGallons = double.Parse(row["capacity_gallons"].ToString());
                if (row["name"] != DBNull.Value)
                    vehicle.name = row["name"].ToString();
                if (row["model"] != DBNull.Value)
                    vehicle.model = row["model"].ToString();
                if (row["operational"] != DBNull.Value)
                    vehicle.operational = bool.Parse(row["operational"].ToString());
                if (row["physical_size"] != DBNull.Value)
                    vehicle.physicalSize = int.Parse(row["physical_size"].ToString());
                vehicles.Add(vehicle);
            }
            return vehicles;
        }

        public static List<Route> ConvertDataTableToRoutesList(DataTable dataTable)
        {
            List<Route> routes = new List<Route>();

            foreach (DataRow row in dataTable.Rows)
            {
                Location origin = new Location();
                if (row["origin_location_id"] != DBNull.Value)
                    origin = (GetLocations("id", row["origin_location_id"].ToString())).First();

                Route route = new Route(origin);
                if (row["id"] != DBNull.Value)
                    route.id = int.Parse(row["id"].ToString());
                if (row["total_time"] != DBNull.Value)
                    route.totalTime = TimeSpan.Parse(row["total_time"].ToString());
                if (row["origin_location_address"] != DBNull.Value)
                    route.origin.address = row["origin_location_address"].ToString();
                if (row["origin_location_id"] != DBNull.Value)
                    route.origin.id = int.Parse(row["origin_location_id"].ToString());
                if (row["date"] != DBNull.Value)
                    route.date = DateTime.Parse(row["date"].ToString());
                if (row["distance_miles"] != DBNull.Value)
                    route.distanceMiles = int.Parse(row["distance_miles"].ToString());
                if (row["maps_url"] != DBNull.Value)
                    route.mapsUrl = row["maps_url"].ToString();
                routes.Add(route);
            }
            return routes;
        }

        public static List<Route> GetRoutes(string columnName = "id", string filterString = null)
        {
            DataTable dataTable = GetRouteInformationData();
            List<Route> routes = ConvertDataTableToRoutesList(dataTable);
            return routes;
        }

        public static string GetAddressByCoordinates(double lat, double lng)
        {
            NpgsqlConnection connection = null;
            string address = "";
            using (connection = new Npgsql.NpgsqlConnection(conString))
            {
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("select_address_by_coordinates", connection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_lat", NpgsqlTypes.NpgsqlDbType.Integer, lat);
                    cmd.Parameters.AddWithValue("p_lng", NpgsqlTypes.NpgsqlDbType.Integer, lng);
                    connection.Open();
                    address = cmd.ExecuteScalar().ToString();
                    connection.Close();

                }
                catch (Exception exception)
                {

                     Logger.Error("Unable to retreive address from coordinates" + lat + "," + lng);
                    Logger.Error(exception.ToString());
                }
            }
            return address;
        }

        public static void UpsertApiMetadata()
        {
            try
            {
                using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
                {
                    string today = DateTime.Today.ToString("yyyy-MM-dd");
                    NpgsqlCommand cmd = new NpgsqlCommand("upsert_api_metadata", connection);
                    cmd.Parameters.AddWithValue("p_call_date", NpgsqlTypes.NpgsqlDbType.Date, today);
                    cmd.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to api call data.");
                Logger.Error(exception.ToString());
            }
        }

        public static void UpdateDistanceFromSource(List<Location> locations)
        {
            foreach (Location location in locations)
            {
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("update_location");
                    cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, location.id);
                    cmd.Parameters.AddWithValue("p_distance_from_source", NpgsqlTypes.NpgsqlDbType.Double, location.distanceFromDepot);
                    RunStoredProcedure(cmd);
                }
                catch (Exception exception)
                {
                    Logger.Error("Unable to append distance data.");
                    Logger.Error(exception.ToString());
                }
            }
        }


        public static void UpdateFeature(string name, bool enabled)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("update_features");
                cmd.Parameters.AddWithValue("p_feature_name", NpgsqlTypes.NpgsqlDbType.Varchar, name);
                cmd.Parameters.AddWithValue("p_enabled", NpgsqlTypes.NpgsqlDbType.Boolean, enabled);
                RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to update features.");
                Logger.Error(exception.ToString());
            }
        }



        public static void UpdateMatrixWeight(List<Location> locations)
        {
            foreach (Location location in locations)
            {
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("update_location");
                    cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, location.id);
                    RouteCalculator calculator = new RouteCalculator();
                    double matrixWeight = calculator.CalculateWeight(location);
                    cmd.Parameters.AddWithValue("p_matrix_weight", NpgsqlTypes.NpgsqlDbType.Double, matrixWeight);
                    RunStoredProcedure(cmd);
                }
                catch (Exception exception)
                {
                    Logger.Error("Unable to append Matrix Weight data for the RouteCalculator Class.");
                    Logger.Error(exception.ToString());
                }
            }
        }

        public static void UpdateDaysUntilDue()
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("update_days_until_due");
                RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to update days until due for the RouteCalculator Class.");
                Logger.Error(exception.ToString());
            }
        }

        public static void UpdateMatrixWeight(int id)
        {
            Location location = GetLocationById(id);
            List<Location> locationList = new List<Location>();
            locationList.Add(location);
            UpdateMatrixWeight(locationList);
        }

        public static void UpdateGpsCoordinates(string address, int id)
        {
            ApiLocation location = new ApiLocation(address);

            //this corresponds to the middle of the Atlantic ocean so it should be safe
            if (location.latitude == 0 && location.longitude == 0)
                return;

            using (NpgsqlCommand cmd = new NpgsqlCommand("update_location"))
            {
                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                cmd.Parameters.AddWithValue("p_coordinates_latitude", NpgsqlTypes.NpgsqlDbType.Double, location.latitude);
                cmd.Parameters.AddWithValue("p_coordinates_longitude", NpgsqlTypes.NpgsqlDbType.Double, location.longitude);

                RunStoredProcedure(cmd);
            }
        }

        public static void DeleteRouteDependencies(int id)
        {
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route_location;"))
                    RunSqlCommandText(cmd);

                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route;"))
                    RunSqlCommandText(cmd);

                using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route_location where location_id = " + id + ";"))
                    RunSqlCommandText(cmd);
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete_location"))
                {
                    cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                    RunStoredProcedure(cmd);
                }
            }

            catch (Exception e)
            {
                Logger.Error(string.Format("Failed to delete route dependencies for route id {0}", id));

                Logger.Error(e.ToString());
            }
        }

        public static void insertRoutes(int batchId, List<Route> routes, Guid activityId)
        {
            foreach (Route route in routes)
            {
                route.id = GetNextRouteId();
                try
                {
                    //ApiRoute apiRoute = new ApiRoute(route);

                    using (NpgsqlCommand cmd = new NpgsqlCommand("insert_route"))
                    {
                        cmd.Parameters.AddWithValue("p_batch_id", NpgsqlTypes.NpgsqlDbType.Integer, batchId);
                        cmd.Parameters.AddWithValue("p_total_time", NpgsqlTypes.NpgsqlDbType.Interval, route.totalTime);
                        cmd.Parameters.AddWithValue("p_origin_location_id", NpgsqlTypes.NpgsqlDbType.Integer, route.origin.id);
                        cmd.Parameters.AddWithValue("p_route_date", NpgsqlTypes.NpgsqlDbType.TimestampTZ, route.date);
                        cmd.Parameters.AddWithValue("p_distance_miles", NpgsqlTypes.NpgsqlDbType.Double, route.distanceMiles);
                        cmd.Parameters.AddWithValue("p_vehicle_id", NpgsqlTypes.NpgsqlDbType.Integer, route.assignedVehicle.id);
                        //cmd.Parameters.AddWithValue("p_maps_url", NpgsqlTypes.NpgsqlDbType.Varchar, apiRoute.mapsUrl);
                        cmd.Parameters.AddWithValue("p_average_location_distance_miles", NpgsqlTypes.NpgsqlDbType.Double, route.averageLocationDistance);
                        cmd.Parameters.AddWithValue("p_activity_id", NpgsqlTypes.NpgsqlDbType.Uuid, activityId);

                        RunStoredProcedure(cmd);
                    }
                    //TO DO insert into route_location linking table, drop down asp.net to view full route
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.ToString());
                }

                try
                {

                    int insertOrder = 0;
                    InsertRouteLocation(route.origin.id, insertOrder, route.id);

                    foreach (Location waypoint in route.waypoints)
                        InsertRouteLocation(waypoint.id, insertOrder += 1, route.id);

                    //insert the route origin since every route returns to HQ
                    InsertRouteLocation(route.origin.id, insertOrder += 1, route.id);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.ToString());
                }
            }
        }

        public static int GetLatestBatchId()
        {
            int id = GetNextRouteBatchId() - 1;
            return id;
        }

        public static void UpdateRouteMetadata(int batchId, RouteCalculator.Metadata metadata)
        {
            try
            {
                UpdateRouteBatchMetadata(batchId, metadata.intakeLocations.Count, metadata.processedLocations.Count, metadata.orphanedLocations.Count, metadata.routesLengthMiles, metadata.routesDuration, metadata.averageRouteDistanceMiles, metadata.averageRouteDistanceStdDev);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString());
            }
        }
    }
}