﻿using NLog;
using Npgsql;
using System;
using System.Threading;
using System.Web.UI;


namespace RouteNavigation
{
    public partial class _Config : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        protected Config Config;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindData();
            }
        }

        protected void BindData()
        {
            try
            {
                Config = DataAccess.GetConfig();
                if (Config.Features.prioritizeNearestLocation)
                    txtChkPrioritizeNearestLocation.Checked = true;
                if (Config.Features.vehicleFillLevel)
                    txtChkVehicleFillLevel.Checked = true;
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
                if (txtChkPrioritizeNearestLocation.Checked == true)
                    DataAccess.UpdateFeature("prioritize_nearest_location", true);
                else
                    DataAccess.UpdateFeature("prioritize_nearest_location", false);
                if (txtChkVehicleFillLevel.Checked == true)
                    DataAccess.UpdateFeature("vehicle_fill_level", true);
                else
                    DataAccess.UpdateFeature("vehicle_fill_level", false);

                NpgsqlCommand cmd = new NpgsqlCommand("upsert_config");
                if (txtOriginLocationId.Text != null && txtOriginLocationId.Text != "")
                    cmd.Parameters.AddWithValue("p_origin_location_id", NpgsqlTypes.NpgsqlDbType.Integer, txtOriginLocationId.Text);
                if (txtMinimumDaysUntilPickup.Text != null && txtMinimumDaysUntilPickup.Text != "")
                    cmd.Parameters.AddWithValue("p_minimum_days_until_pickup", NpgsqlTypes.NpgsqlDbType.Integer, txtMinimumDaysUntilPickup.Text);
                if (txtMatrixPriorityMultiplier.Text != null && txtMatrixPriorityMultiplier.Text != "")
                    cmd.Parameters.AddWithValue("p_matrix_priority_multiplier", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixPriorityMultiplier.Text);
                if (txtMatrixDaysUntilDueExponent.Text != null && txtMatrixDaysUntilDueExponent.Text != "")
                    cmd.Parameters.AddWithValue("p_matrix_days_until_due_exponent", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixDaysUntilDueExponent.Text);
                if (txtMatrixDistanceFromSource.Text != null && txtMatrixDistanceFromSource.Text != "")
                    cmd.Parameters.AddWithValue("p_matrix_distance_from_source", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixDistanceFromSource.Text);
                if (txtMatrixOverDueMultiplier.Text != null && txtMatrixOverDueMultiplier.Text != "")
                    cmd.Parameters.AddWithValue("p_matrix_overdue_multiplier", NpgsqlTypes.NpgsqlDbType.Double, txtMatrixOverDueMultiplier.Text);
                if (txtCurrentFillLevelErrorMargin.Text != null && txtCurrentFillLevelErrorMargin.Text != "")
                    cmd.Parameters.AddWithValue("p_current_fill_level_error_margin", NpgsqlTypes.NpgsqlDbType.Double, txtCurrentFillLevelErrorMargin.Text);
                if (txtRouteDistanceMaxMiles.Text != null && txtRouteDistanceMaxMiles.Text != "")
                    cmd.Parameters.AddWithValue("p_route_distance_max_miles", NpgsqlTypes.NpgsqlDbType.Double, txtRouteDistanceMaxMiles.Text);
                if (txtMaximumDaysOverdue.Text != null && txtMaximumDaysOverdue.Text != "")
                    cmd.Parameters.AddWithValue("p_maximum_days_overdue", NpgsqlTypes.NpgsqlDbType.Integer, txtMaximumDaysOverdue.Text);
                if (txtWorkDayStart.Text != null && txtWorkDayStart.Text != "")
                    cmd.Parameters.AddWithValue("p_workday_start_time", NpgsqlTypes.NpgsqlDbType.Time, txtWorkDayStart.Text);
                if (txtWorkDayEnd.Text != null && txtWorkDayEnd.Text != "")
                    cmd.Parameters.AddWithValue("p_workday_end_time", NpgsqlTypes.NpgsqlDbType.Time, txtWorkDayEnd.Text);
                if (txtOilPickupAverageDuration.Text != null && txtOilPickupAverageDuration.Text != "")
                    cmd.Parameters.AddWithValue("p_oil_pickup_average_duration", NpgsqlTypes.NpgsqlDbType.Interval, TimeSpan.FromMinutes(int.Parse(txtOilPickupAverageDuration.Text.ToString())));
                if (txtGreasePickupAverageDuration.Text != null && txtGreasePickupAverageDuration.Text != "")
                    cmd.Parameters.AddWithValue("p_grease_pickup_average_duration", NpgsqlTypes.NpgsqlDbType.Interval, TimeSpan.FromMinutes(int.Parse(txtGreasePickupAverageDuration.Text.ToString())));

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