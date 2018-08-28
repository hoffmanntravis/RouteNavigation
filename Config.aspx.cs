using NLog;
using Npgsql;
using System;
using System.Threading;
using System.Web.UI;


namespace RouteNavigation
{
    public partial class _Config : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private Config Config;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindData();
            }
        }

        private void BindData()
        {
            try
            {
                Config = DataAccess.GetConfig();
                if (Config.Features.prioritizeNearestLocation)
                    chkPrioritizeNearestLocation.Checked = true;
                if (Config.Features.vehicleFillLevel)
                    chkVehicleFillLevel.Checked = true;
                if (Config.Features.geneticAlgorithmGrowthDecayExponent)
                    chkGrowthDecayExponent.Checked = true;

                txtCurrentFillLevelErrorMargin.Text = Config.Calculation.currentFillLevelErrorMarginPercent.ToString();
                txtOilPickupAverageDuration.Text = Config.Calculation.oilPickupAverageDurationMinutes.ToString();
                txtGreasePickupAverageDuration.Text = Config.Calculation.greasePickupAverageDurationMinutes.ToString();
                txtMinimumDaysUntilPickup.Text = Config.Calculation.minimDaysUntilPickup.ToString();
                txtMaximumDaysOverdue.Text = Config.Calculation.maximumDaysOverdue.ToString();

                if (Config.Calculation.workdayStartTime != DateTime.MinValue)
                    txtWorkDayStart.Text = Config.Calculation.workdayStartTime.TimeOfDay.ToString();
                if (Config.Calculation.workdayEndTime != DateTime.MinValue)
                    txtWorkDayEnd.Text = Config.Calculation.workdayEndTime.TimeOfDay.ToString();

                txtRouteDistanceMaxMiles.Text = Config.Calculation.routeDistanceMaxMiles.ToString();
                if (Config.Calculation.origin != null)
                    txtOriginLocationId.Text = Config.Calculation.origin.id.ToString();
                txtMatrixOverDueMultiplier.Text = Config.Matrix.overDueMultiplier.ToString();
                txtMatrixDaysUntilDueExponent.Text = Config.Matrix.daysUntilDueExponent.ToString();
                txtMatrixDistanceFromSource.Text = Config.Matrix.distanceFromSourceMultiplier.ToString();
                txtMatrixPriorityMultiplier.Text = Config.Matrix.priorityMultiplier.ToString();

                if (Config.Calculation.origin != null)
                {
                    txtOriginName.Text = Config.Calculation.origin.locationName;
                    txtOriginAddress.Text = Config.Calculation.origin.address;
                }

                txtIterations.Text = Config.GeneticAlgorithm.Iterations.ToString();
                txtPopulationSize.Text = Config.GeneticAlgorithm.PopulationSize.ToString();
                txtNeighborCount.Text = Config.GeneticAlgorithm.NeighborCount.ToString();
                txtTournamentSize.Text = Config.GeneticAlgorithm.TournamentSize.ToString();
                txtTournamentWinnerCount.Text = Config.GeneticAlgorithm.TournamentWinnerCount.ToString();
                txtBreederCount.Text = Config.GeneticAlgorithm.BreederCount.ToString();
                txtOffspringPoolSize.Text = Config.GeneticAlgorithm.OffspringPoolSize.ToString();
                txtCrossoverProbability.Text =  Config.GeneticAlgorithm.CrossoverProbability.ToString();
                txtElitismRatio.Text = Config.GeneticAlgorithm.ElitismRatio.ToString();
                txtMutationProbability.Text = Config.GeneticAlgorithm.MutationProbability.ToString();
                txtMutationAlleleMax.Text = Config.GeneticAlgorithm.MutationAlleleMax.ToString();
                txtGrowthDecayExponent.Text =  Config.GeneticAlgorithm.GrowthDecayExponent.ToString();

            }
            catch (Exception exception)
            {
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = exception.Message;
                Logger.Error(exception);
            }
        }

        protected void BtnUpdateSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(String.IsNullOrEmpty(txtOriginLocationId.Text)))
                {
                    int textId = int.Parse(txtOriginLocationId.Text);
                    DataAccess.SetOrigin(textId);
                }
                else
                {
                    Exception exception = new Exception("Origin field is null.  Please set the depot to an id in the Locations table / page, or calculation will fail.  Aborting config update.");
                    throw exception;
                }

                //This section / data structure could use a rework to be less explicitly mapped to the db keys
                if (chkPrioritizeNearestLocation.Checked == true)
                    DataAccess.UpdateFeature("prioritize_nearest_location", true);
                else
                    DataAccess.UpdateFeature("prioritize_nearest_location", false);
                if (chkVehicleFillLevel.Checked == true)
                    DataAccess.UpdateFeature("vehicle_fill_level", true);
                else
                    DataAccess.UpdateFeature("vehicle_fill_level", false);

                if (chkGrowthDecayExponent.Checked == true)
                    DataAccess.UpdateFeature("genetic_algorithm_growth_decay_exponent", true);
                else
                    DataAccess.UpdateFeature("genetic_algorithm_growth_decay_exponent", false);

                NpgsqlCommand cmd = new NpgsqlCommand("upsert_config");
                if (!(String.IsNullOrEmpty(txtOriginLocationId.Text)))
                    cmd.Parameters.AddWithValue("p_origin_location_id", NpgsqlTypes.NpgsqlDbType.Integer, txtOriginLocationId.Text);

                if (!(String.IsNullOrEmpty(txtMatrixPriorityMultiplier.Text)))
                    cmd.Parameters.AddWithValue("p_matrix_priority_multiplier", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixPriorityMultiplier.Text);
                if (!(String.IsNullOrEmpty(txtMatrixDaysUntilDueExponent.Text)))
                    cmd.Parameters.AddWithValue("p_matrix_days_until_due_exponent", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixDaysUntilDueExponent.Text);
                if (!(String.IsNullOrEmpty(txtMatrixDistanceFromSource.Text)))
                    cmd.Parameters.AddWithValue("p_matrix_distance_from_source", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixDistanceFromSource.Text);
                if (!(String.IsNullOrEmpty(txtMatrixOverDueMultiplier.Text)))
                    cmd.Parameters.AddWithValue("p_matrix_overdue_multiplier", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixOverDueMultiplier.Text);

                if (!(String.IsNullOrEmpty(txtMinimumDaysUntilPickup.Text)))
                    cmd.Parameters.AddWithValue("p_minimum_days_until_pickup", NpgsqlTypes.NpgsqlDbType.Integer, txtMinimumDaysUntilPickup.Text);
                if (!(String.IsNullOrEmpty(txtCurrentFillLevelErrorMargin.Text)))
                    cmd.Parameters.AddWithValue("p_current_fill_level_error_margin", NpgsqlTypes.NpgsqlDbType.Double, txtCurrentFillLevelErrorMargin.Text);
                if (!(String.IsNullOrEmpty(txtRouteDistanceMaxMiles.Text)))
                    cmd.Parameters.AddWithValue("p_route_distance_max_miles", NpgsqlTypes.NpgsqlDbType.Double, txtRouteDistanceMaxMiles.Text);
                if (!(String.IsNullOrEmpty(txtMaximumDaysOverdue.Text)))
                    cmd.Parameters.AddWithValue("p_maximum_days_overdue", NpgsqlTypes.NpgsqlDbType.Integer, txtMaximumDaysOverdue.Text);
                if (!(String.IsNullOrEmpty(txtWorkDayStart.Text)))
                    cmd.Parameters.AddWithValue("p_workday_start_time", NpgsqlTypes.NpgsqlDbType.Time, txtWorkDayStart.Text);
                if (!(String.IsNullOrEmpty(txtWorkDayEnd.Text)))
                    cmd.Parameters.AddWithValue("p_workday_end_time", NpgsqlTypes.NpgsqlDbType.Time, txtWorkDayEnd.Text);
                if (!(String.IsNullOrEmpty(txtOilPickupAverageDuration.Text)))
                    cmd.Parameters.AddWithValue("p_oil_pickup_average_duration", NpgsqlTypes.NpgsqlDbType.Interval, TimeSpan.FromMinutes(int.Parse(txtOilPickupAverageDuration.Text)));
                if (!(String.IsNullOrEmpty(txtGreasePickupAverageDuration.Text)))
                    cmd.Parameters.AddWithValue("p_grease_pickup_average_duration", NpgsqlTypes.NpgsqlDbType.Interval, TimeSpan.FromMinutes(int.Parse(txtGreasePickupAverageDuration.Text)));

                if (!(String.IsNullOrEmpty(txtIterations.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_iterations", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtIterations.Text));
                if (!(String.IsNullOrEmpty(txtPopulationSize.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_population_size", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtPopulationSize.Text));
                if (!(String.IsNullOrEmpty(txtNeighborCount.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_neighbor_count", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtNeighborCount.Text));
                if (!(String.IsNullOrEmpty(txtTournamentSize.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_tournament_size", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtTournamentSize.Text));
                if (!(String.IsNullOrEmpty(txtTournamentWinnerCount.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_tournament_winner_count", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtTournamentWinnerCount.Text));
                if (!(String.IsNullOrEmpty(txtBreederCount.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_breeder_count", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtBreederCount.Text));
                if (!(String.IsNullOrEmpty(txtOffspringPoolSize.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_offspring_pool_size", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtOffspringPoolSize.Text));
                if (!(String.IsNullOrEmpty(txtCrossoverProbability.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_crossover_probability", NpgsqlTypes.NpgsqlDbType.Double, double.Parse(txtCrossoverProbability.Text));
                if (!(String.IsNullOrEmpty(txtElitismRatio.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_elitism_ratio", NpgsqlTypes.NpgsqlDbType.Double, double.Parse(txtElitismRatio.Text));
                if (!(String.IsNullOrEmpty(txtMutationProbability.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_mutation_probability", NpgsqlTypes.NpgsqlDbType.Double, double.Parse(txtMutationProbability.Text));
                if (!(String.IsNullOrEmpty(txtMutationAlleleMax.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_mutation_allele_max", NpgsqlTypes.NpgsqlDbType.Integer, int.Parse(txtMutationAlleleMax.Text));
                if (!(String.IsNullOrEmpty(txtGrowthDecayExponent.Text)))
                    cmd.Parameters.AddWithValue("p_genetic_algorithm_growth_decay_exponent", NpgsqlTypes.NpgsqlDbType.Double, double.Parse(txtGrowthDecayExponent.Text));

                DataAccess.RunStoredProcedure(cmd);
                BindData();
            }
            catch (Exception exception)
            {
                dataValidation.IsValid = false;
                dataValidation.ErrorMessage = exception.Message;
                Logger.Error(exception);
            }
        }
    }
}