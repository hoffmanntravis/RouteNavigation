using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    public class DataAccess
    {
        static protected string apiKey = System.Configuration.ConfigurationManager.AppSettings["googleApiKey"];
        static protected string mapsBaseUrl = System.Configuration.ConfigurationManager.AppSettings["googleDirectionsMapsUrl"];
        static protected string illegalCharactersString = System.Configuration.ConfigurationManager.AppSettings["googleApiIllegalCharacters"];
        static protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        public void UpdateDbConfigWithApiStrings()
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                char[] illegalCharacters = illegalCharactersString.ToCharArray();
                NpgsqlCommand cmd = new NpgsqlCommand("upsert_config");
                cmd.Parameters.AddWithValue("p_google_directions_maps_url", NpgsqlTypes.NpgsqlDbType.Varchar, mapsBaseUrl);
                cmd.Parameters.AddWithValue("p_google_api_key", NpgsqlTypes.NpgsqlDbType.Varchar, apiKey);
                cmd.Parameters.AddWithValue("p_google_api_illegal_characters", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Char, illegalCharacters);
                RunStoredProcedure(cmd);
            }
        }

        public int GetNextRouteId()
        {
            int id;
            using (NpgsqlCommand cmd = new NpgsqlCommand("select_next_route_id"))
                id = int.Parse(ReadStoredProcedureAsString(cmd));
            return id;
        }

        public int GetNextRouteBatchId()
        {
            int id;
            using (NpgsqlCommand cmd = new NpgsqlCommand("select_next_route_batch_id"))
                id = int.Parse(ReadStoredProcedureAsString(cmd));
            return id;
        }

        public int RunStoredProcedure(NpgsqlCommand cmd)
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

        public void UpdateRouteBatchMetadata(int id, int locationsIntakeCount, int locationsProcessedCount, int locationsOrphanedCount, double totalDistanceMiles, TimeSpan totalTime)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("update_route_batch_metadata"))
            {
                cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, id);
                cmd.Parameters.AddWithValue("p_locations_intake_count", NpgsqlTypes.NpgsqlDbType.Integer, locationsIntakeCount);
                cmd.Parameters.AddWithValue("p_locations_processed_count", NpgsqlTypes.NpgsqlDbType.Integer, locationsProcessedCount);
                cmd.Parameters.AddWithValue("p_locations_orphaned_count", NpgsqlTypes.NpgsqlDbType.Integer, locationsOrphanedCount);
                cmd.Parameters.AddWithValue("p_total_distance_miles", NpgsqlTypes.NpgsqlDbType.Double, totalDistanceMiles);
                cmd.Parameters.AddWithValue("p_total_time", NpgsqlTypes.NpgsqlDbType.Interval, totalTime);
                RunStoredProcedure(cmd);
            }
        }



        public void InsertRouteBatch()
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("insert_route_batch"))
            {
                RunStoredProcedure(cmd);
            }
        }

        public void InsertRouteLocation(int locationId, int insertOrder, int routeId)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("insert_route_location"))
            {
                cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
                cmd.Parameters.AddWithValue("p_location_id", NpgsqlTypes.NpgsqlDbType.Integer, locationId);
                cmd.Parameters.AddWithValue("p_insert_order", NpgsqlTypes.NpgsqlDbType.Integer, insertOrder);
                RunStoredProcedure(cmd);
            }
        }

        public int RunSqlCommandText(NpgsqlCommand cmd)
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

        public string RunPostgreExport(string tableName, string csvData)
        {
            using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString))
            {
                connection.Open();
                csvData = connection.BeginTextExport("COPY " + tableName + " TO STDOUT DELIMITER ',' CSV HEADER").ReadToEnd();
                connection.Close();
            }
            return csvData;
        }

        public void RunPostgreImport(string tableName, string Content)
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

        public bool ReadStoredProcedureIntoDataTable(NpgsqlCommand cmd, DataTable dataTable)
        {
            Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(conString);

            cmd.Connection = connection;
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();

            NpgsqlDataReader reader = cmd.ExecuteReader();
            dataTable.Load(reader);

            connection.Close();
            return true;

        }

        public string ReadStoredProcedureAsString(NpgsqlCommand cmd)
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

        public DataTable GetVehicleData(string columnName = "name", string filterString = null)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_vehicle_with_filter");

                cmd.Parameters.AddWithValue("p_column_name", NpgsqlTypes.NpgsqlDbType.Text, columnName);
                if (filterString == null)
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, filterString);

                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString(), "ERROR");
            }
            return dataTable;
        }

        public DataTable GetLocationData(DataTable dataTable, string columnName = "location_name", string filterString = null)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_location_with_filter");

                cmd.Parameters.AddWithValue("p_column_name", NpgsqlTypes.NpgsqlDbType.Text, columnName);
                if (filterString == null || filterString == "")
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Text, filterString);

                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString(), "ERROR");
            }
            return dataTable;
        }

        public DataTable GetLocationData(int id)
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
                Logging.Logger.LogMessage(exception.ToString(), "ERROR");
            }
            return dataTable;
        }

        public DataTable GetConfigData()
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_config");
                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString(), "ERROR");
            }
            return dataTable;
        }

        public DataTable GetFeaturesData()
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("select_features");
                ReadStoredProcedureIntoDataTable(cmd, dataTable);
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString(), "ERROR");
            }
            return dataTable;
        }

        public DataTable GetRouteInformationData()
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_information");
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public DataTable getRouteBatchData()
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_batch");
            ReadStoredProcedureIntoDataTable(cmd, dataTable);
            return dataTable;
        }

        internal DataTable GetRouteDetailsData(int routeId)
        {
            DataTable dataTable = new DataTable();

            NpgsqlCommand cmd = new NpgsqlCommand("select_route_details");
            cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public DataTable GetRouteData(int id)
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_by_id");
            cmd.Parameters.AddWithValue("p_filter_string", NpgsqlTypes.NpgsqlDbType.Integer, id);
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public List<Location> GetLocations(string columnName = "location_name", string filterString = null)
        {
            DataTable dataTable = new DataTable();
            GetLocationData(dataTable, columnName, filterString);
            List<Location> list = ConvertDataTableToLocationsList(dataTable);
            return list;
        }

        public Config GetConfig()
        {
            DataTable configsDataTable = GetConfigData();
            DataTable featuresDataTable = GetFeaturesData();

            Config config = ConvertDataTablesToConfig(configsDataTable, featuresDataTable);
            return config;
        }

        public Location GetLocationById(int id)
        {
            UpdateDaysUntilDue();
            DataTable dataTable = GetLocationData(id);

            List<Location> list = ConvertDataTableToLocationsList(dataTable);
            if (list.Count > 0)
                return list.First();
            else
                return null;
        }

        public Config ConvertDataTablesToConfig(DataTable configs, DataTable features)
        {
            Config config = new Config();
            foreach (DataRow row in configs.Rows)
            {
                if (row["origin_location_id"] != DBNull.Value)
                    config.Calculation.OriginLocationId = int.Parse(row["origin_location_id"].ToString());
                if (row["minimum_days_until_pickup"] != DBNull.Value)
                    config.Calculation.minimDaysUntilPickup = int.Parse(row["minimum_days_until_pickup"].ToString());
                if (row["route_max_hours"] != DBNull.Value)
                    config.Calculation.routeMaxHours = double.Parse(row["route_max_hours"].ToString());
                if (row["current_fill_level_error_margin"] != DBNull.Value)
                    config.Calculation.currentFillLevelErrorMarginPercent = double.Parse(row["current_fill_level_error_margin"].ToString());
                if (row["grease_pickup_average_duration"] != DBNull.Value)
                    config.Calculation.oilPickupAverageDurationMinutes = TimeSpan.Parse(row["grease_pickup_average_duration"].ToString()).TotalMinutes;
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
                    config.maximumDaysOverdue = int.Parse(row["maximum_days_overdue"].ToString());
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

        public List<Location> ConvertDataTableToLocationsList(DataTable dataTable)
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
                    location.lat = double.Parse(row["coordinates_latitude"].ToString());
                if (row["coordinates_longitude"] != DBNull.Value)
                    location.lng = double.Parse(row["coordinates_longitude"].ToString());
                if (row["distance_from_source"] != DBNull.Value)
                    location.distanceFromSource = double.Parse(row["distance_from_source"].ToString());
                if (row["pickup_interval_days"] != DBNull.Value)
                    location.pickupIntervalDays = int.Parse(row["pickup_interval_days"].ToString());
                if (row["matrix_weight"] != DBNull.Value)
                    location.matrixWeight = double.Parse(row["matrix_weight"].ToString());
                if (row["contact_name"] != DBNull.Value)
                    location.contactName = row["contact_name"].ToString();
                if (row["contact_email"] != DBNull.Value)
                    location.contactEmail = row["contact_email"].ToString();
                if (row["visit_time"] != DBNull.Value)
                    location.visitTime = TimeSpan.Parse(row["visit_time"].ToString());
                locations.Add(location);
            }
            return locations;
        }

        public List<Vehicle> GetVehicles(string columnName = "name", string filterString = null)
        {
            DataTable dataTable = GetVehicleData(columnName, filterString);
            List<Vehicle> vehicles = ConvertDataTableToVehiclesList(dataTable);
            return vehicles;
        }

        public List<Vehicle> ConvertDataTableToVehiclesList(DataTable dataTable)
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

        public List<Route> ConvertDataTableToRoutesList(DataTable dataTable)
        {
            List<Route> routes = new List<Route>();

            foreach (DataRow row in dataTable.Rows)
            {
                Location origin = new Location();
                if (row["origin_location_id"] != DBNull.Value)
                    origin = (GetLocations("id", row["origin_location_id"].ToString())).First();

                Location destination = new Location();
                if (row["origin_location_id"] != DBNull.Value)
                    destination = (GetLocations("id", row["destination_location_id"].ToString())).First();

                Route route = new Route(origin, destination);
                if (row["id"] != DBNull.Value)
                    route.id = int.Parse(row["id"].ToString());
                if (row["total_time"] != DBNull.Value)
                    route.totalTime = TimeSpan.Parse(row["total_time"].ToString());
                if (row["destination_location_id"] != DBNull.Value)
                    route.destination.id = int.Parse(row["destination_location_id"].ToString());
                if (row["destination_location_address"] != DBNull.Value)
                    route.destination.address = row["destination_location_address"].ToString();
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

        public List<Route> GetRoutes(string columnName = "id", string filterString = null)
        {
            DataTable dataTable = GetRouteInformationData();
            List<Route> routes = ConvertDataTableToRoutesList(dataTable);
            return routes;
        }

        public string GetAddressByCoordinates(double lat, double lng)
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
                    Logging.Logger.LogMessage("Unable to retreive address from coordinates" + lat + "," + lng, "ERROR");
                    Logging.Logger.LogMessage(exception.ToString());
                }
            }
            return address;
        }

        public void UpsertApiMetadata()
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
                Logging.Logger.LogMessage("Unable to api call data.", "ERROR");
                Logging.Logger.LogMessage(exception.ToString());
            }
        }

        public void UpdateDistanceFromSource(List<Location> locations)
        {
            foreach (Location location in locations)
            {
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("update_location");
                    cmd.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Integer, location.id);
                    cmd.Parameters.AddWithValue("p_distance_from_source", NpgsqlTypes.NpgsqlDbType.Double, location.distanceFromSource);
                    RunStoredProcedure(cmd);
                }
                catch (Exception exception)
                {
                    Logging.Logger.LogMessage("Unable to append distance data.", "ERROR");
                    Logging.Logger.LogMessage(exception.ToString());
                }
            }
        }


        public void UpdateFeature(string name, bool enabled)
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
                Logging.Logger.LogMessage("Unable to update features.", "ERROR");
                Logging.Logger.LogMessage(exception.ToString());
            }
        }



        public void UpdateMatrixWeight(List<Location> locations)
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
                    Logging.Logger.LogMessage("Unable to append Matrix Weight data for the RouteCalculator Class.", "ERROR");
                    Logging.Logger.LogMessage(exception.ToString());
                }
            }
        }

        public void UpdateDaysUntilDue()
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("update_days_until_due");
                RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage("Unable to update days until due for the RouteCalculator Class.", "ERROR");
                Logging.Logger.LogMessage(exception.ToString());
            }
        }

        public void UpdateMatrixWeight(int id)
        {
            Location location = GetLocationById(id);
            List<Location> locationList = new List<Location>();
            locationList.Add(location);
            UpdateMatrixWeight(locationList);
        }

        public void UpdateGpsCoordinates(string address, int id)
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

        public void DeleteRouteDependencies(int id)
        {
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

        public void insertRoutes(int batchId, List<Route> routes)
        {
            foreach (Route route in routes)
            {
                route.id = GetNextRouteId();
                try
                {
                    ApiRoute apiRoute = new ApiRoute(route);

                    using (NpgsqlCommand cmd = new NpgsqlCommand("insert_route"))
                    {
                        cmd.Parameters.AddWithValue("p_batch_id", NpgsqlTypes.NpgsqlDbType.Integer, batchId);
                        cmd.Parameters.AddWithValue("p_total_time", NpgsqlTypes.NpgsqlDbType.Interval, route.totalTime);
                        cmd.Parameters.AddWithValue("p_origin_location_id", NpgsqlTypes.NpgsqlDbType.Integer, route.origin.id);
                        cmd.Parameters.AddWithValue("p_destination_location_id", NpgsqlTypes.NpgsqlDbType.Integer, route.destination.id);
                        cmd.Parameters.AddWithValue("p_route_date", NpgsqlTypes.NpgsqlDbType.TimestampTZ, route.date);
                        cmd.Parameters.AddWithValue("p_distance_miles", NpgsqlTypes.NpgsqlDbType.Double, route.distanceMiles);
                        cmd.Parameters.AddWithValue("p_vehicle_id", NpgsqlTypes.NpgsqlDbType.Integer, route.assignedVehicle.id);
                        cmd.Parameters.AddWithValue("p_maps_url", NpgsqlTypes.NpgsqlDbType.Varchar, apiRoute.mapsUrl);

                        RunStoredProcedure(cmd);
                    }
                    //TO DO insert into route_location linking table, drop down asp.net to view full route
                }
                catch (Exception exception)
                {
                    Logging.Logger.LogMessage(exception.ToString(), "ERROR");
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
                    Logging.Logger.LogMessage(exception.ToString(), "ERROR");
                }
            }
        }

        public void UpdateRouteMetadata(int batchId, RouteCalculator.Metadata metadata)
        {
            try
            {
                UpdateRouteBatchMetadata(batchId, metadata.intakeLocations.Count, metadata.processedLocations.Count, metadata.orphanedLocations.Count, metadata.routesLengthMiles, metadata.routesDuration);
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString(), "ERROR");
            }
        }
    }
}