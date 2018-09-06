using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RouteNavigation
{
    public class Config
    {
        public static class Matrix
        {
            public static double priorityMultiplier = 1;
            public static double daysUntilDueExponent = 1;
            public static double distanceFromSourceMultiplier = 1;
            public static double overDueMultiplier = 1;
        }

        public static class Calculation
        {
            public static Location origin;
            public static double currentFillLevelErrorMarginPercent = 0;
            public static DateTime workdayStartTime = DateTime.MinValue;
            public static DateTime workdayEndTime = DateTime.MinValue;
            public static double oilPickupAverageDurationMinutes = 30;
            public static double greasePickupAverageDurationMinutes = 30;
            public static int minimDaysUntilPickup = 0;
            public static int maximumDaysOverdue = 60;
            public static double nearbyLocationDistance = 20;
            public static double averageCityTravelSpeed = 15;
            public static double averageHighwayTravelSpeed = 50;
            public static TimeSpan dropOffTime = TimeSpan.FromMinutes(60);
            public static double minimumSearchDistance = 5;
            public static double localRadiusTolerancePercent = .25;
            public static double routeDistanceMaxMiles = 50;
        }

        public static class GeneticAlgorithm
        {
            public static int Iterations = 50;
            public static int PopulationSize = 100;
            public static int NeighborCount = 200;
            public static int TournamentSize = 10;
            public static int TournamentWinnerCount = 1;
            public static int BreederCount = 4;
            public static int OffspringPoolSize = 2;
            public static double CrossoverProbability = .25;
            public static double ElitismRatio = .001;
            public static double MutationProbability = .05;
            public static int MutationAlleleMax = 1;
            public static double GrowthDecayExponent = 1;
        }

        public static class Features
        {
            public static bool vehicleFillLevel = false;
            public static bool prioritizeNearestLocation = false;
            public static bool geneticAlgorithmGrowthDecayExponent = false;
        }
    }


}