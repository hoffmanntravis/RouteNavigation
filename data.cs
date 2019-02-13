﻿using Dapper;
using Dapper.FluentMap;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        static DataAccess()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            FluentMapper.Initialize(config => {
                config.AddMap(new CoordinatesMap());
            });
        }

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
                Logger.Error(exception);
            }
            return dataTable;
        }

        public static DataTable GetLocationData(DataTable dataTable, string filterColumnName = null, string filterString = null, string columnSortString = null, bool ascending = true)
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
                Logger.Error(exception);
            }
            return dataTable;
        }

        static string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;

            var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);
            return attrib == null ? null : attrib.Description;
        }

        public static List<Location> GetLocations()
        {
            List<Location> locations = new List<Location>();

            using (var connection = new Npgsql.NpgsqlConnection(conString))
                //return connection.Query<Location>("select_location", commandType: CommandType.StoredProcedure).ToList();

                //mapping example in case a join or multi query is needed to map this
                locations = connection.Query<Location, Coordinates, Location> ("select_location", (l, coordinates) => {
                    l.coordinates = coordinates;
                    return l;
                }, splitOn: "coordinates_latitude", commandType: CommandType.StoredProcedure).ToList();

            return locations;
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
                Logger.Error(exception);
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
                Logger.Error(exception);
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
                Logger.Error(exception);
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
                Logger.Error(exception);
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

        public static DataTable GetRouteBatchData()
        {
            DataTable dataTable = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("select_route_batch");
            ReadStoredProcedureIntoDataTable(cmd, dataTable);
            return dataTable;
        }

        public static DataTable GetRouteDetailsData(int routeId, bool excludeOrigin = false)
        {
            DataTable dataTable = new DataTable();

            NpgsqlCommand cmd = new NpgsqlCommand("select_route_details");
            cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
            cmd.Parameters.AddWithValue("p_exclude_origin", NpgsqlTypes.NpgsqlDbType.Boolean, excludeOrigin);
            ReadStoredProcedureIntoDataTable(cmd, dataTable);

            return dataTable;
        }

        public static void updateRouteLocation(int locationId, int routeId, int order)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("update_route_location");

            cmd.Parameters.AddWithValue("p_location_id", NpgsqlTypes.NpgsqlDbType.Integer, locationId);
            cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
            cmd.Parameters.AddWithValue("p_order", NpgsqlTypes.NpgsqlDbType.Integer, order);

            RunStoredProcedure(cmd);
        }

        public static DataTable GetRouteDetailsData(bool excludeOrigin = false)
        {
            DataTable dataTable = new DataTable();

            NpgsqlCommand cmd = new NpgsqlCommand("select_route_details");
            cmd.Parameters.AddWithValue("p_exclude_origin", NpgsqlTypes.NpgsqlDbType.Boolean, excludeOrigin);
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

        public static void PopulateConfig()
        {
                DataTable configsDataTable = GetConfigData();
                DataTable featuresDataTable = GetFeaturesData();

                ConvertDataTablesToConfig(configsDataTable, featuresDataTable);
        }

        public static Location GetLocationById(int id)
        {
            UpdateDaysUntilDue();
            Location location;
            using (var connection = new Npgsql.NpgsqlConnection(conString))
            {
                try
                {
                    location = connection.Query<Location, Coordinates, Location>("select_location_by_id", (l, coordinates) => {
                        l.coordinates = coordinates;
                        return l;
                    }, new { p_id = id }, splitOn: "coordinates_latitude", commandType: CommandType.StoredProcedure).FirstOrDefault();
                    return location;
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                    return null;
                }
            }
        }

        public static void SetOrigin(int id)
        {
            DataTable configsDataTable = GetConfigData();

            foreach (DataRow row in configsDataTable.Rows)
            {
                Config.Calculation.origin = GetLocationById(id);

                if (Config.Calculation.origin == null)
                {
                    Exception exception = new Exception(String.Format("Unable to resolve origin Id [{0}] to a location.  Please update the ID to a valid ID.", id));
                    Logger.Error(exception);
                    throw exception;
                }
                else
                {
                    Logger.Debug(String.Format("Set origin to id: {0}", Config.Calculation.origin.id));
                }
            }

        }


        public static void ConvertDataTablesToConfig(DataTable configs, DataTable features)
        {
            foreach (DataRow row in configs.Rows)
            {
                if (row["origin_location_id"] != DBNull.Value)
                {
                    SetOrigin(int.Parse(row["origin_location_id"].ToString()));
                }

                if (row["minimum_days_until_pickup"] != DBNull.Value)
                    Config.Calculation.minimDaysUntilPickup = uint.Parse(row["minimum_days_until_pickup"].ToString());
                if (row["current_fill_level_error_margin"] != DBNull.Value)
                    Config.Calculation.currentFillLevelErrorMarginPercent = double.Parse(row["current_fill_level_error_margin"].ToString());
                if (row["oil_pickup_average_duration"] != DBNull.Value)
                    Config.Calculation.oilPickupAverageDurationMinutes = TimeSpan.Parse(row["oil_pickup_average_duration"].ToString()).TotalMinutes;
                if (row["grease_pickup_average_duration"] != DBNull.Value)
                    Config.Calculation.greasePickupAverageDurationMinutes = TimeSpan.Parse(row["grease_pickup_average_duration"].ToString()).TotalMinutes;
                if (row["maximum_days_overdue"] != DBNull.Value)
                    Config.Calculation.maximumDaysOverdue = uint.Parse(row["maximum_days_overdue"].ToString());
                if (row["workday_start_time"] != DBNull.Value)
                    Config.Calculation.workdayStartTime = TimeSpan.Parse(row["workday_start_time"].ToString());
                if (row["workday_end_time"] != DBNull.Value)
                    Config.Calculation.workdayEndTime = TimeSpan.Parse(row["workday_end_time"].ToString());
                if (row["grease_pickup_time_cutoff"] != DBNull.Value)
                    Config.Calculation.greaseTrapCutoffTime = DateTime.Parse(row["grease_pickup_time_cutoff"].ToString());
                if (row["max_distance_from_depot"] != DBNull.Value)
                    Config.Calculation.maxDistanceFromDepot = double.Parse(row["max_distance_from_depot"].ToString());
                if (row["search_minimum_distance"] != DBNull.Value)
                    Config.Calculation.searchMinimumDistance = double.Parse(row["search_minimum_distance"].ToString());
                if (row["search_radius_percent"] != DBNull.Value)
                    Config.Calculation.searchRadiusFraction = double.Parse(row["search_radius_percent"].ToString());
                if (row["genetic_algorithm_iterations"] != DBNull.Value)
                    Config.GeneticAlgorithm.Iterations = uint.Parse(row["genetic_algorithm_iterations"].ToString());
                if (row["genetic_algorithm_population_size"] != DBNull.Value)
                    Config.GeneticAlgorithm.PopulationSize = uint.Parse(row["genetic_algorithm_population_size"].ToString());
                if (row["genetic_algorithm_neighbor_count"] != DBNull.Value)
                    Config.GeneticAlgorithm.NeighborCount = uint.Parse(row["genetic_algorithm_neighbor_count"].ToString());
                if (row["genetic_algorithm_tournament_size"] != DBNull.Value)
                    Config.GeneticAlgorithm.TournamentSize = uint.Parse(row["genetic_algorithm_tournament_size"].ToString());
                if (row["genetic_algorithm_tournament_winner_count"] != DBNull.Value)
                    Config.GeneticAlgorithm.TournamentWinnerCount = uint.Parse(row["genetic_algorithm_tournament_winner_count"].ToString());
                if (row["genetic_algorithm_breeder_count"] != DBNull.Value)
                    Config.GeneticAlgorithm.BreederCount = uint.Parse(row["genetic_algorithm_breeder_count"].ToString());
                if (row["genetic_algorithm_offspring_pool_size"] != DBNull.Value)
                    Config.GeneticAlgorithm.OffspringPoolSize = uint.Parse(row["genetic_algorithm_offspring_pool_size"].ToString());
                if (row["genetic_algorithm_crossover_probability"] != DBNull.Value)
                    Config.GeneticAlgorithm.CrossoverProbability = double.Parse(row["genetic_algorithm_crossover_probability"].ToString());
                if (row["genetic_algorithm_elitism_ratio"] != DBNull.Value)
                    Config.GeneticAlgorithm.ElitismRatio = double.Parse(row["genetic_algorithm_elitism_ratio"].ToString());
                if (row["genetic_algorithm_mutation_probability"] != DBNull.Value)
                    Config.GeneticAlgorithm.MutationProbability = double.Parse(row["genetic_algorithm_mutation_probability"].ToString());
                if (row["genetic_algorithm_mutation_allele_max"] != DBNull.Value)
                    Config.GeneticAlgorithm.MutationAlleleMax = uint.Parse(row["genetic_algorithm_mutation_allele_max"].ToString());
                if (row["genetic_algorithm_growth_decay_exponent"] != DBNull.Value)
                    Config.GeneticAlgorithm.GrowthDecayExponent = double.Parse(row["genetic_algorithm_growth_decay_exponent"].ToString());
            }

            foreach (DataRow row in features.Rows)
            {
                /*
                if (row["feature_name"] as string == "prioritize_nearest_location")
                    Config.Features.prioritizeNearestLocation = bool.Parse(row["enabled"].ToString());
                */
                if (row["feature_name"] as string == "vehicle_fill_level")
                    Config.Features.vehicleFillLevel = bool.Parse(row["enabled"].ToString());
                if (row["feature_name"] as string == "genetic_algorithm_growth_decay_exponent")
                    Config.Features.geneticAlgorithmGrowthDecayExponent = bool.Parse(row["enabled"].ToString());
                if (row["feature_name"] as string == "locations_jetting_exclude_from_calc")
                    Config.Features.locationsJettingExcludeFromCalc = bool.Parse(row["enabled"].ToString());
                if (row["feature_name"] as string == "locations_jetting_remove_on_import")
                    Config.Features.locationsJettingRemoveOnImport = bool.Parse(row["enabled"].ToString());
            }
        }

        internal static void deleteLocationsWildCardSearch(string searchString)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("delete_location_wildcard");
            cmd.Parameters.AddWithValue("p_string", NpgsqlTypes.NpgsqlDbType.Varchar, searchString);
            RunStoredProcedure(cmd);
        }
        internal static void updateGreaseCutoffToConfigValue()
        {
            NpgsqlCommand cmd = new NpgsqlCommand("update_grease_cutoff_to_config_value");
            RunStoredProcedure(cmd);
        }

        public static List<Location> ConvertRouteDetailsDataTableToLocations(DataTable dataTable)
        {
            List<Location> locations = new List<Location>();
            foreach (DataRow row in dataTable.Rows)
            {
                Location location = new Location();
                if (row["route_id"] != DBNull.Value)
                    location.routeId = int.Parse(row["route_id"].ToString());
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
                if (row["coordinates_latitude"] != DBNull.Value)
                    location.coordinates.lat = double.Parse(row["coordinates_latitude"].ToString());
                if (row["coordinates_longitude"] != DBNull.Value)
                    location.coordinates.lng = double.Parse(row["coordinates_longitude"].ToString());
                if (row["distance_from_source"] != DBNull.Value)
                    location.distanceFromDepot = double.Parse(row["distance_from_source"].ToString());
                if (row["has_oil"] != DBNull.Value)
                    location.hasOil = (bool)row["has_oil"];
                if (row["has_grease"] != DBNull.Value)
                    location.hasGrease = (bool)row["has_grease"];
                locations.Add(location);
            }
            return locations;
        }

        public class IterationStatus
        {
            public int currentIteration;
            public int totalIterations;
        }

        public static IterationStatus GetCalcStatus()
        {
            DataTable dataTable = new DataTable();

            IterationStatus iterationStatus = new IterationStatus();
            NpgsqlCommand cmd = new NpgsqlCommand("select_latest_route_batch");
            ReadStoredProcedureIntoDataTable(cmd, dataTable);
            foreach (DataRow row in dataTable.Rows)
            {
                Location location = new Location();
                if (row["iteration_current"] != DBNull.Value)
                    iterationStatus.currentIteration = int.Parse(row["iteration_current"].ToString());
                if (row["iteration_total"] != DBNull.Value)
                    iterationStatus.totalIterations = int.Parse(row["iteration_total"].ToString());
            }
            return iterationStatus;
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

        public static string GetAddressByCoordinates(double lat, double lng)
        {
            string address = "";
            using (var connection = new Npgsql.NpgsqlConnection(conString))
            {
                try
                {
                    address = connection.Query<string>("select_address_by_coordinates", commandType: CommandType.StoredProcedure).ToString();
                }
                catch (Exception exception)
                {
                    Logger.Error("Unable to retreive address from coordinates" + lat + "," + lng);
                    Logger.Error(exception);
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
                Logger.Error(exception);
            }
        }

        public static void UpdateDistanceFromSource(List<Location> locations)
        {
            foreach (Location location in locations)
                UpdateDistanceFromSource(location);
        }

        public static void UpdateDistanceFromSource(Location location)
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
                Logger.Error(exception);
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
                Logger.Error(exception);
            }
        }

        public static void cleanupNullBatchCalcs()
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("delete_null_route_batch");
                string statusString = ReadStoredProcedureAsString(cmd);
                RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to cleanup null batches.");
                Logger.Error(exception);
            }
        }

        public static void updateCancellationStatus(bool cancel = true)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("update_route_batch_cancellation_status");
            cmd.Parameters.AddWithValue("p_cancellation_request", NpgsqlTypes.NpgsqlDbType.Boolean, cancel);
            RunStoredProcedure(cmd);
        }
        public static Boolean getCancellationStatus()
        {
            NpgsqlCommand cmd = new NpgsqlCommand("get_route_batch_cancellation_status");
            return Boolean.Parse(ReadStoredProcedureAsString(cmd));
        }

        public static void updateIteration(uint currentIteration, uint totalIterations)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("update_iteration");
                cmd.Parameters.AddWithValue("p_iteration_current", NpgsqlTypes.NpgsqlDbType.Integer, currentIteration);
                cmd.Parameters.AddWithValue("p_iteration_total", NpgsqlTypes.NpgsqlDbType.Integer, totalIterations);
                RunStoredProcedure(cmd);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to update route iterations.");
                Logger.Error(exception);
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
                Logger.Error(exception);
            }
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
                //using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route_location;"))
                //    RunSqlCommandText(cmd);

                //using (NpgsqlCommand cmd = new NpgsqlCommand("delete FROM route;"))
                //    RunSqlCommandText(cmd);

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

                Logger.Error(e);
            }
        }

        public static void DeleteLocationFromRouteLocation(int routeId, int locationId)
        {
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand("delete_location_from_route_location;"))
                {
                    cmd.Parameters.AddWithValue("p_route_id", NpgsqlTypes.NpgsqlDbType.Integer, routeId);
                    cmd.Parameters.AddWithValue("p_location_id", NpgsqlTypes.NpgsqlDbType.Integer, locationId);
                    RunStoredProcedure(cmd);
                }
            }

            catch (Exception e)
            {
                Logger.Error(string.Format("Failed to delete location from for route id {0}", routeId));
                Logger.Error(e);
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
                        cmd.Parameters.AddWithValue("p_origin_location_id", NpgsqlTypes.NpgsqlDbType.Integer, Config.Calculation.origin.id);
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
                    Logger.Error(exception);
                }

                try
                {
                    int insertOrder = 0;
                    InsertRouteLocation(Config.Calculation.origin.id, insertOrder += 1, route.id);

                    foreach (Location waypoint in route.waypoints)
                        InsertRouteLocation(waypoint.id, insertOrder += 1, route.id);

                    //insert the route origin since every route returns to HQ
                    InsertRouteLocation(Config.Calculation.origin.id, insertOrder += 1, route.id);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
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
                Logger.Error(exception);
            }
        }
    }
}