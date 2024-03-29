﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RouteNavigation
{
    public class Config
    {

        public static class Calculation
        {
            public static Location origin;
            public static double currentFillLevelErrorMarginPercent = 0;
            public static TimeSpan workdayStartTime = TimeSpan.Parse("06:00:00");
            public static TimeSpan workdayEndTime = TimeSpan.Parse("20:00:00");
            public static DateTime greaseTrapCutoffTime = DateTime.MinValue;
            public static double oilPickupAverageDurationMinutes = 30;
            public static double greasePickupAverageDurationMinutes = 30;
            public static uint maximumDaysOverdue = 60;
            public static double nearbyLocationDistance = 20;
            public static double averageCityTravelSpeed = 15;
            public static double averageHighwayTravelSpeed = 50;
            public static TimeSpan dropOffTime = TimeSpan.FromMinutes(10);
            public static double searchMinimumDistance = 5;
            public static double searchRadiusFraction = .25;
            public static double maxDistanceFromDepot = 100;

            public static double GreaseEarlyServiceRatio = .1;
            public static double OilEarlyServiceRatio = .05;
        }

        public static class GeneticAlgorithm
        {
            public static uint Iterations = 50;
            public static uint PopulationSize = 100;
            public static uint NeighborCount = 200;
            public static uint TournamentSize = 10;
            public static uint TournamentWinnerCount = 1;
            public static uint BreederCount = 4;
            public static uint OffspringPoolSize = 2;
            public static double CrossoverProbability = .25;
            public static double ElitismRatio = .001;
            public static double MutationProbability = .05;
            public static uint MutationAlleleMax = 1;
            public static double GrowthDecayExponent = 1;
            public static double seedRatioNearestNeighbor = .20;
        }

        public static class Features
        {
            //public static bool prioritizeNearestLocation = false;
            public static bool vehicleFillLevel = false;
            public static bool geneticAlgorithmGrowthDecayExponent = false;
            public static bool locationsJettingExcludeFromCalc = true;
            public static bool excludeGreaseLocationsOver500FromCalc = true;
            public static bool locationsJettingRemoveOnImport = false;
        }
    }


}