using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RouteNavigation
{
    public class Config
    {
        public Matrix matrix = new Matrix();
        public Calculation Calculation = new Calculation();
        public Features Features = new Features();
    }

    public class Matrix
    {
        public double priorityMultiplier = 1;
        public double daysUntilDueExponent = 1;
        public double distanceFromSourceMultiplier = 1;
        public double overDueMultiplier = 1;
    }

    public class Calculation
    {
        public static Location origin;
        public static double currentFillLevelErrorMarginPercent = 0;
        public static DateTime workdayStartTime = DateTime.MinValue;
        public static DateTime workdayEndTime = DateTime.MinValue;
        public static double oilPickupAverageDurationMinutes = 30;
        public static double greasePickupAverageDurationMinutes = 30;
        public static int minimDaysUntilPickup = 0;
        public static int maximumDaysOverdue = 60;
        public static double routeDistanceMaxMiles = 50;
        public static double nearbyLocationDistance = 20;
        public static double averageCityTravelSpeed = 30;
        public static double averageHighwayTravelSpeed = 70;
    }

    public class Features
    {
        public bool vehicleFillLevel = false;
        public bool prioritizeNearestLocation = false;
    }


}