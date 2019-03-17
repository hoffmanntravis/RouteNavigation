using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NLog;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace RouteNavigation
{
    public class RouteCalculator
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public Metadata metadata = new Metadata();
        public List<Route> routes = new List<Route>();
        /*
        private List<Vehicle> currentVehicles;
        private List<Location> orphanedLocations;
        private List<Location> serviceNowLocations;
        private List<Location> compatibleLocations;
        private Route potentialRoute;
        private DateTime currentTime;
        private DateTime potentialTime;
        private Location previousLocation;
        private Location nextLocation;
        private Vehicle vehicle;
        private Location nearestLocation;
        private List<Location> availableLocationsWithPostponedLocations;
        private List<Location> postPonedLocations;
        private Location firstDueLocation;
        private double currentDistance;
        private double potentialDistance;
        private DateTime startTime;
        private DateTime endTime;

        private double nextLocationDistanceMiles;
        private double distanceTolerance;
        private double distanceToDepotFromLastWaypoint;
        private TimeSpan travelTimeBackToDepot;
        private TimeSpan travelTime;
        */

        private Task task;
        public Guid activityId;

        public uint neighborCount = 60;
        private static object addLock = new object();
        
        public RouteCalculator(List<Location> locations, List<Vehicle> vehicles)
        {
            CalculateRoutes(locations.ToList(), vehicles.ToList());
            
            try
            {
                task = new Task(() => CalculateRoutes(locations, vehicles));
                task.Start();

            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
            
        }
        
        public void Stop()
        {
            task.Wait();
        }

        public void CalculateRoutes(List<Location> availableLocations, List<Vehicle> availableVehicles)
        {
            Logger.Trace("Calculate Routes called");
            availableLocations = availableLocations.DeepClone();

            activityId = Trace.CorrelationManager.ActivityId;
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();

            List<Location> orphanedLocations = new List<Location>();
            DateTime startDate = AdvanceDateToNextWeekday(System.DateTime.Now.Date);
            metadata.intakeLocations = availableLocations.ToList();
            DateTime startTime = startDate + Config.Calculation.workdayStartTime;
            DateTime endTime = startDate + Config.Calculation.workdayEndTime;
            List<Vehicle> currentVehicles = new List<Vehicle>(availableVehicles);
            availableLocations = NearestNeighbor(availableLocations);

            try
            {
                //This should get moved to the genetic algorithm as it is a static answer and recalculating is wasteful

                //int hash1 = GenerateRouteHash(availableLocations);
                if (Config.Calculation.origin == null)
                {
                    Exception exception = new Exception("Origin is null.  Please set it in the config page, or calculation will fail.");
                    Logger.Error(exception);
                    throw exception;
                }

                
                while (availableLocations.Count > 0)
                {
                    DateTime currentTime = startTime;
                    Route potentialRoute = new Route();
                    if (currentVehicles.Count == 0)
                    {
                        //get some more vehicles and start a new day, with new routes
                        currentVehicles = availableVehicles.ToList();
                        currentTime = AdvanceDateToNextWeekday(currentTime);
                    }
                    Logger.Trace("Current Time is {0}", currentTime);

                    //Remove any locations that would be picked up too soon to be relevent.  We'll invoke a recursive call at the end to deal with these.
                    List<Location> availableLocationsWithPostponedLocations = availableLocations.ToList();
                    List<Location> postPonedLocations = GetLaterDateLocations(availableLocations, currentTime).ToList();
                    availableLocations = availableLocations.Except(postPonedLocations).ToList();

                    //If all that is left are locations that need to be processed later, advance the date accordingly
                    if (postPonedLocations.Count > 0 && availableLocations.Count == 0)
                    {
                        Location firstDueLocation = postPonedLocations.OrderBy(a => a.DaysUntilDue).First();
                        double daysElapsed = firstDueLocation.DaysElapsed.Value;
                        currentVehicles = availableVehicles.ToList();
                        currentTime = currentTime.AddDays(daysElapsed);

                        availableLocations = postPonedLocations.ToList();
                        if (currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday)
                            currentTime = AdvanceDateToNextWeekday(currentTime);
                        continue;
                    }

                    List<Location> serviceNowLocations = GetRequireServiceNowLocations(availableLocations, currentTime);
                    if (serviceNowLocations.Count > 0)
                    {
                        availableLocations = availableLocations.Except(serviceNowLocations).ToList();
                        availableLocations.InsertRange(0, serviceNowLocations);
                    }

                    //sort vehicles by size descending.  We do this to ensure that large vehicles are handled first since they have a limited location list available to them.
                    currentVehicles.OrderBy(a => a.physicalSize);
                    Vehicle vehicle = currentVehicles.First();
                    List<Location> compatibleLocations = GetCompatibleLocations(vehicle, availableLocations);
                    double currentDistance = 0;

                    potentialRoute.AllLocations.Add(Config.Calculation.origin);

                    Location previousLocation = Config.Calculation.origin;
                    while (compatibleLocations.Count() > 0)
                    {
                        
                        verifyLocationOrder(potentialRoute.Waypoints);
                        DateTime potentialTime = currentTime;
                        double potentialDistance = currentDistance;

                        Location nextLocation = compatibleLocations.First();

                        if (nextLocation.IntendedPickupDate != null)
                            throw new Exception("This should not be populated yet");
                        
                        if (compatibleLocations.Count > 1)
                        {
                            Location nearestLocation = FindNearestLocation(previousLocation, compatibleLocations);
                            if (previousLocation.distanceToNearestLocation != null)
                                if (previousLocation.distanceToNearestLocation == 0)
                                    nextLocation = previousLocation.nearestLocation;
                        }
                        
                        compatibleLocations.Remove(nextLocation);
                        availableLocations.Remove(nextLocation);

                        
                        if (Config.Features.vehicleFillLevel == true)
                        {
                            nextLocation.CurrentGallonsEstimate = EstimateLocationGallons(nextLocation);
                            if (!(CheckVehicleCanAcceptMoreLiquid(vehicle, nextLocation)))
                            {
                                Logger.Trace(String.Format("Performing a dropoff.  This will take {0} minutes.  Resetting current gallons to 0.", Config.Calculation.dropOffTime));

                                vehicle.currentGallons = 0;
                                potentialTime += Config.Calculation.dropOffTime;
                            }
                        }

                        double nextLocationDistanceMiles = CalculateDistance(previousLocation, nextLocation);
                        double distanceTolerance = nextLocation.DistanceFromSource * (Config.Calculation.searchRadiusFraction);
                        lock (addLock)
                            if (potentialRoute.Waypoints.Count > 0)
                            {
                                if (nextLocationDistanceMiles >= Math.Max(distanceTolerance, Config.Calculation.searchMinimumDistance))
                                {
                                    Logger.Trace(String.Format("Removing location {1} from compatible locations. Distance from {1} to {0} is greater than the radius search tolerance of {2} miles.", nextLocation.Account, previousLocation.Account, distanceTolerance));
                                    continue;
                                }
                                else
                                    Logger.Trace(String.Format("Distance from {1} to {0} is less than the radius search tolerance of {2} miles.  Will not remove from compatible locations.", nextLocation.Account, previousLocation.Account, distanceTolerance));
                            }
                            else
                                Logger.Trace(String.Format("Route currently has 0 locations.  Adding {0} to populate the route.", nextLocation));

                        TimeSpan travelTime = CalculateTravelTime(nextLocationDistanceMiles);

                        Logger.Trace(String.Format("Travel time from {0} ({1}) to next location {2} ({3}) is {4} minutes", previousLocation.Account, previousLocation.Address, nextLocation.Account, nextLocation.Address, travelTime.TotalMinutes));

                        potentialTime += travelTime;
                        
                        if (nextLocation.OilPickupNextDate != null)
                            potentialTime += TimeSpan.FromMinutes(Config.Calculation.oilPickupAverageDurationMinutes);

                        if (nextLocation.GreaseTrapPickupNextDate != null)
                            potentialTime += TimeSpan.FromMinutes(Config.Calculation.greasePickupAverageDurationMinutes);

                        //get the current total distance, including the trip back to the depot for comparison to max distance setting
                        potentialRoute.DistanceMiles += nextLocationDistanceMiles;

                        lock (addLock)
                            if (potentialRoute.DistanceMiles is double.NaN)
                            {
                                Logger.Error(String.Format("Locations are {0} and {1} with gps coordinates of {2}:{3} and {4}:{5}", Config.Calculation.origin.Account, nextLocation.Account, Config.Calculation.origin.Coordinates.Lat, Config.Calculation.origin.Coordinates.Lng, nextLocation.Coordinates.Lat, nextLocation.Coordinates.Lng));
                                Logger.Error("potentialRoute.distanceMiles is null");
                            }

                        //double localRadiusTolerance = nextLocation.distanceFromDepot / localRadiusDivisor;
                        //This is only relevent if we have a waypoint in the route.  Otherwise, we may end up with no valid locations.  
                        lock (addLock)
                            if (potentialRoute.Waypoints.Count > 0)
                            {
                                //if the location is within a certain radius, even if it means the day length being exceeded
                                if (potentialTime > endTime)
                                {
                                    Logger.Trace(String.Format("Removing location {0}.  Adding this location would put the route time at {1} which is later than {2}", nextLocation.Account, potentialTime, endTime));
                                    continue;
                                }
                            }

                        //Made it past any checks that would preclude this nearest route from getting added, add it as a waypoint on the route
                        if (Config.Features.vehicleFillLevel == true)
                            vehicle.currentGallons += nextLocation.CurrentGallonsEstimate;

                        if (nextLocation.IntendedPickupDate != null)
                            throw new Exception("this should be null!!");
                        nextLocation.IntendedPickupDate = currentTime;

                        potentialRoute.Waypoints.Add(nextLocation);

                        currentTime = potentialTime;
                        previousLocation = nextLocation;
                    }

                    //Add the time to travel back to the depot
                    double distanceToDepotFromLastWaypoint = CalculateDistance(previousLocation, Config.Calculation.origin);
                    TimeSpan travelTimeBackToDepot = CalculateTravelTime(distanceToDepotFromLastWaypoint);
                    Logger.Trace(String.Format("Travel time back from {0} ({1}) to {2} ({3}) is {4} minutes", previousLocation.Account, previousLocation.Address, Config.Calculation.origin.Account, Config.Calculation.origin.Address, travelTimeBackToDepot.TotalMinutes));
                    currentTime.Add(travelTimeBackToDepot);

                        potentialRoute.AllLocations.AddRange(potentialRoute.Waypoints.ToList());
                    potentialRoute.AllLocations.Add(Config.Calculation.origin);

                        if (potentialRoute.Waypoints.Count == 0)
                            throw new Exception("Route waypoints count is 0.  Something went wrong.  This is probably caused by invalid condidtions removing all compatible locations.  This may be a function of the data set or possibly a programming bug.  Discuss with Developer if possible.");

                        potentialRoute.AssignedVehicle = vehicle;
                        potentialRoute.Waypoints.ForEach(r => r.AssignedVehicle = vehicle);
                    currentVehicles.Remove(vehicle);
                        potentialRoute.Date = startDate;

                    
                    potentialRoute.TotalTime = currentTime - startTime;
                    //int oilLocationsCount = potentialRoute.AllLocations.Where(a => a.OilPickupNextDate != null).ToList().Count;
                    //int greaseLocationsCount = potentialRoute.AllLocations.Where(a => a.GreaseTrapPickupNextDate != null).ToList().Count;
                    //Logger.Debug(String.Format("there are {0} oil locations and {1} grease locations.", oilLocationsCount, greaseLocationsCount));
                    
                    potentialRoute.DistanceMiles = CalculateTotalDistance(potentialRoute.AllLocations, true);
                    Logger.Trace("TSP calculated a shortest route 'flight' distance of " + potentialRoute.DistanceMiles);
                    potentialRoute.AverageLocationDistance = CalculateAverageLocationDistance(potentialRoute);
                    routes.Add(potentialRoute);
                    availableLocations = availableLocationsWithPostponedLocations.Except(potentialRoute.Waypoints).ToList();
                }

                foreach (Route route in routes.ToList())
                {
                    
                    metadata.processedLocations.AddRange(route.Waypoints.ToList());
                    metadata.routesDuration += route.TotalTime;
                    metadata.routesLengthMiles += route.DistanceMiles;

                    if (metadata.routesLengthMiles is double.NaN)
                        Logger.Error("metadata.routesLengthMiles is null");
                }
                if (metadata.processedLocations.Count == 0)
                    throw new Exception("Unable to create any routes.");

                if (routes.Count > 0)
                {
                    metadata.averageRouteDistanceMiles = CalculateAverageRouteDistance(routes);
                    metadata.averageRouteDistanceStdDev = CalculateRoutesStdDev(routes);
                    metadata.fitnessScore = metadata.CalculateFitnessScore();
                }

                else
                    Logger.Error("Unable to create any routes.");
                
                metadata.orphanedLocations = availableLocations.Where(x => !metadata.processedLocations.Any(y => y.Address == x.Address)).ToList();
            }

            catch (Exception e)
            {
                Logger.Error(e);
                throw e;
            }

        }

        private bool verifyLocationOrder(List<Location> listLocation)
        {
                listLocation = listLocation.ToList();
                for (int x = 1; x < listLocation.Count - 1; x++)
                    if (listLocation[x].IntendedPickupDate > listLocation[x + 1].IntendedPickupDate)
                        throw new Exception("Dates are out of order");
                return true;
        }


        private DateTime AdvanceDateToNextWeekday(DateTime date)
        {
                date = date.AddDays(1);

                if (date.DayOfWeek == DayOfWeek.Saturday)
                    date = date.AddDays(2);
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    date = date.AddDays(1);
                return date;
        }

        public Location FindFarthestLocation(Location source, List<Location> locations)
        {
                double farthestDistance = 0;
                Location farthestLocation = new Location();
                foreach (Location location in locations)
                {
                    double thisDistance = CalculateDistance(source, location);
                    if (thisDistance >= farthestDistance)
                    {
                        farthestLocation = location;
                        farthestDistance = thisDistance;
                    }
                }
                return farthestLocation;
        }

        public double CalculateTotalDistance(List<Location> locations, bool roundTrip = false)
        {
                double totalDistance = 0;

                for (int x = 0; x < locations.Count - 1; x++)
                    totalDistance += CalculateDistance(locations[x], locations[x + 1]);

                if (roundTrip)
                    totalDistance += CalculateDistance(locations[locations.Count - 1], locations[0]);

                return totalDistance;
        }

        public List<Location> ThreeOptSwap(List<Location> route)
        {
                {
                    double previousBestDistance;
                    double bestDistance;
                    int iterations = 0;
                    int routeHashStart = GenerateRouteHash(route);
                    do
                    {
                        //add the depot back in to ensure the route is shortest with the depot included
                        previousBestDistance = CalculateTotalDistance(route, true);
                        bestDistance = double.MaxValue;
                        for (int i = 0; i < route.Count - 1; i++)
                        {
                            for (int j = i; j < route.Count - 1; j++)
                            {
                                for (int k = j; k < route.Count; k++)
                                {
                                    List<Location> newRoute = RunThreeOptSwap(route, i, j, k);
                                    if (routeHashStart != GenerateRouteHash(newRoute))
                                        throw new Exception("hashes do not match!");
                                    double newDistance = CalculateTotalDistance(newRoute, true);
                                    if (newDistance < previousBestDistance)
                                    {
                                        bestDistance = newDistance;
                                        route = newRoute;
                                    }
                                }
                            }
                        }
                        iterations++;
                    }
                    while (bestDistance < previousBestDistance);
                    Logger.Trace("Ran " + iterations + " iterations of ThreeOpt TSP");
                    return route;
                }

        }

        public List<Location> TwoOptSwap(List<Location> route)
        {
                double previousBestDistance;
                double bestDistance;
                int iterations = 0;

                do
                {
                    //add the depot back in to ensure the route is shortest with the depot included

                    previousBestDistance = CalculateTotalDistance(route, true);

                    bestDistance = double.MaxValue;
                    for (int i = 0; i < route.Count - 1; i++)
                    {
                        for (int j = i; j < route.Count; j++)
                        {
                            List<Location> newRoute = RunTwoOptSwap(route, i, j);

                            double newDistance = CalculateTotalDistance(newRoute, true);
                            if (newDistance < previousBestDistance)
                            {

                                bestDistance = newDistance;

                                route = newRoute;
                            }
                        }
                    }
                    iterations++;
                }
                while (bestDistance < previousBestDistance);
                Logger.Debug("Ran " + iterations + " iterations of TwoOpt TSP");
                return route;
        }

        public List<Location> RunThreeOptSwap(List<Location> locations, int i, int j, int k)
        {
                List<Location> newRoute = new List<Location>();

                for (int x = 0; x <= i - 1; x++)
                    newRoute.Add(locations[x]);


                List<Location> reverseLocations = new List<Location>();
                for (int x = i; x <= j - 1; x++)
                    reverseLocations.Add(locations[x]);

                reverseLocations.Reverse();
                newRoute.AddRange(reverseLocations);

                reverseLocations = new List<Location>();
                for (int x = j; x <= k; x++)
                    reverseLocations.Add(locations[x]);

                reverseLocations.Reverse();

                newRoute.AddRange(reverseLocations);

                for (int x = k + 1; x < locations.Count; x++)

                    newRoute.Add(locations[x]);

                return newRoute;
        }

        public List<Location> RunTwoOptSwap(List<Location> locations, int i, int j)
        {
                List<Location> newRoute = new List<Location>();
                lock (addLock)
                    for (int x = 0; x <= i - 1; x++)
                        newRoute.Add(locations[x]);

                List<Location> reverseLocations = new List<Location>();
                lock (addLock)
                    for (int x = i; x <= j; x++)
                        reverseLocations.Add(locations[x]);

                lock (addLock)
                    //reverseLocations.Reverse();
                    lock (addLock)
                        newRoute.AddRange(reverseLocations);
                lock (addLock)
                    for (int x = j + 1; x < locations.Count; x++)
                        newRoute.Add(locations[x]);


                return newRoute;
        }

        private int GenerateRouteHash(List<Location> locations)
        {
                int hash = 0;
                try
                {

                    List<Location> locationsCopy = new List<Location>(locations);
                    string concat = "";
                    lock (addLock)
                        locationsCopy.Sort((a, b) => a.Address.CompareTo(b.Address));
                    foreach (Location location in locationsCopy)
                        concat += location.Address;

                    hash = concat.GetHashCode();

                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return hash;
        }

        public Route CalculateTSPRouteNN(Route route)
        {
                try
                {
                    Logger.Trace("Attempting to TSP. Rearranging locations...");
                    lock (addLock)
                        route.Waypoints = NearestNeighbor(route.Waypoints);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return route;
        }

        public Route CalculateTSPRouteTwoOpt(Route route)
        {
                try
                {
                    Logger.Info("Attempting to TSP. Rearranging locations...");
                    lock (addLock)
                        route.Waypoints = TwoOptSwap(route.Waypoints);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return route;
        }

        public Route CalculateTSPRouteThreeOpt(Route route)
        {
                try
                {
                    Logger.Info("Attempting to TSP. Rearranging locations...");
                    lock (addLock)
                        route.Waypoints = ThreeOptSwap(route.Waypoints);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return route;
        }

        public static List<Location> NearestNeighbor(List<Location> route, Location firstNode = null)
        {
                if (route.Count == 1)
                    return route;

                List<Location> nearestNeighborRoute = new List<Location>();
                List<Location> unVisitedNodes = new List<Location>(route);
                Location nearest;
                if (firstNode == null)
                    nearest = unVisitedNodes.First();
                else
                    nearest = firstNode;
                nearestNeighborRoute.Add(nearest);
                unVisitedNodes.Remove(nearest);


                while (unVisitedNodes.Count > 0)
                {
                    nearest = FindNearestLocation(nearest, unVisitedNodes);
                    nearestNeighborRoute.Add(nearest);
                    unVisitedNodes.Remove(nearest);
                }

                //if (routeHashStart != GenerateRouteHash(nearestNeighborRoute))
                //    throw new Exception("hashes do not match!");

                route = nearestNeighborRoute;
                return route;
        }

        public double CalculateAverageRouteDistance(List<Route> routes)
        {
                double average = 0;
                double totalDistance = 0;

                foreach (Route route in routes)
                    totalDistance += route.DistanceMiles;

                average = totalDistance / routes.Count;
                return average;
        }

        public double CalculateAverageLocationDistance(Route route)
        {
                //remove one location, because origin and destination are the same
                double average = route.DistanceMiles / (route.AllLocations.Count - 1);
                return average;
        }

        private double CalculateRoutesStdDev(List<Route> routes)
        {
                List<double> values = new List<double>();

                foreach (Route route in routes)

                    values.Add(route.DistanceMiles);

                double avg = values.Average();

                return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        private bool CheckVehicleCanAcceptMoreLiquid(Vehicle vehicle, Location location)
        {
                //Check if the vehicle can accept more gallons.  Also, multiple the total gallons by a percentage.  Finally, check that the vehicle isn't empty, otherwise we're going to visit regadless.
                if (vehicle.currentGallons + location.CurrentGallonsEstimate > vehicle.oilTankSize * ((100 - Config.Calculation.currentFillLevelErrorMarginPercent) / 100) && vehicle.currentGallons != 0)
                    return false;
                return true;
        }

        private double EstimateLocationGallons(Location location)
        {
                if (location.OilTankSize == null && location.GreaseTrapSize == null && location.Id != Config.Calculation.origin.Id)
                    throw new Exception(String.Format("{0} does not have an oil tank size or grease trap size value configured.  This makes it impossible to estimate current vehicle fill level.  Please disable the feature 'estimate gallons' in the config page.", location.Account));

                if (location.GreaseTrapDaysUntilDue == null && location.OilPickupDaysUntilDue == null)
                    return 0;

                double currentOilEstimate = 0;
                if (location.OilPickupDaysUntilDue > 0 && location.OilPickupCustomer == true)
                    currentOilEstimate = (location.OilPickupDaysUntilDue.Value / location.OilPickupSchedule.Value) * location.OilTankSize.Value;
                else
                    //capacity is assumed to be full if we have lapsed since the last visit
                    currentOilEstimate = location.OilTankSize.Value;

                double currentGreaseEstimate = 0;
                if (location.GreaseTrapDaysUntilDue > 0 && location.GreaseTrapCustomer == true)
                    currentGreaseEstimate = (location.GreaseTrapDaysUntilDue.Value / location.GreaseTrapSchedule.Value) * location.GreaseTrapSize.Value;
                else
                    //capacity is assumed to be full if we have lapsed since the last visit
                    currentGreaseEstimate = location.GreaseTrapSize.Value;

                return currentOilEstimate + currentGreaseEstimate;
        }

        private List<Location> GetLaterDateLocations(List<Location> availableLocations, DateTime currentDate)
        {
                List<Location> laterDateLocations = new List<Location>();

                foreach (Location l in availableLocations)
                {
                    if (l.OilPickupNextDate != null && l.OilPickupLastScheduledService != null)
                        l.OilDaysElapsed = (currentDate - l.OilPickupLastScheduledService.Value).TotalDays;
                    if (l.GreaseTrapPickupNextDate != null && l.GreaseTrapLastScheduledService != null)
                        l.GreaseDaysElapsed = (currentDate - l.GreaseTrapLastScheduledService.Value).TotalDays;

                    if (l.OilDaysElapsed != null && l.GreaseDaysElapsed != null)
                        l.DaysElapsed = Math.Max(l.OilDaysElapsed.Value, l.GreaseDaysElapsed.Value);
                    else if (l.GreaseDaysElapsed == null && l.OilDaysElapsed != null)
                        l.DaysElapsed = l.OilDaysElapsed.Value;
                    else if (l.OilDaysElapsed == null && l.GreaseDaysElapsed != null)
                        l.DaysElapsed = l.GreaseDaysElapsed.Value;
                    else
                        continue;

                    if (l.DaysElapsed < Config.Calculation.MinimumDaysUntilPickup)
                        laterDateLocations.Add(l);
                }

                foreach (Location l in laterDateLocations)
                    if (l.DaysElapsed > Config.Calculation.MinimumDaysUntilPickup)
                        Logger.Error("This should be impossible!!");

                return laterDateLocations;
        }

        private List<Location> GetRequireServiceNowLocations(List<Location> availableLocations, DateTime currentDate)
        {
                List<Location> serviceNowLocations = new List<Location>();
                foreach (Location l in availableLocations)
                {
                    if (currentDate >= l.OilPickupNextDate)
                        serviceNowLocations.Add(l);
                    else if (currentDate >= l.GreaseTrapPickupNextDate)

                        serviceNowLocations.Add(l);
                }
                return serviceNowLocations;
        }

        public class Metadata
        {
            public int locationsHash;
            public double routesLengthMiles;
            public double averageRouteDistanceMiles;
            public double averageRouteDistanceStdDev;
            public TimeSpan routesDuration;
            public List<Location> intakeLocations = new List<Location>();
            public List<Location> orphanedLocations = new List<Location>();
            public List<Location> invalidApiLocations = new List<Location>();
            public List<Location> processedLocations = new List<Location>();
            public double fitnessScore;

            public double CalculateFitnessScore()
            {
                return routesLengthMiles;
            }
        }

        private List<Location> GetCompatibleLocations(Vehicle vehicle, List<Location> locations)
        {
                List<Location> compatibleLocations = locations.Where(l => vehicle.physicalSize <= l.VehicleSize || l.VehicleSize is null).ToList();
                return compatibleLocations;
        }

        public static List<Location> GetPossibleLocations(List<Vehicle> vehicles, List<Location> locations)
        {
                List<Location> possibleLocations = new List<Location>();
                double smallestVehicle;

                smallestVehicle = vehicles.Min(v => v.physicalSize);

                foreach (Location location in locations)
                {
                    if (location.VehicleSize < smallestVehicle)
                    {
                        Logger.Debug(String.Format("{0} is being ignored because it's size of {1} is smaller than the smallest vehicle size of {2}", location.Account, location.VehicleSize, smallestVehicle));
                        //if we find a vehicle that works with the location, add the location to the list of possible locations and break out to the next location
                        continue;
                    }

                    if (location.OilPickupNextDate != null)
                        if ((DateTime.Now - location.OilPickupNextDate).Value.TotalDays > (Config.Calculation.maximumDaysOverdue) && location.OilPickupNextDate != null)
                        {
                            Logger.Debug(String.Format("{0} is being ignored because the oil pickup next date of {1} is more than {2} days overdue", location.Account, location.OilPickupNextDate, Config.Calculation.maximumDaysOverdue));
                            continue;
                        }

                    if (location.GreaseTrapPickupNextDate != null)
                        if ((DateTime.Now - location.GreaseTrapPickupNextDate).Value.TotalDays > (Config.Calculation.maximumDaysOverdue) && location.GreaseTrapPickupNextDate != null)
                        {
                            Logger.Debug(String.Format("{0} is being ignored because the grease pickup next date of {1} is more than {2} days overdue", location.Account, location.GreaseTrapPickupNextDate, Config.Calculation.maximumDaysOverdue));
                            continue;
                        }

                    if (location.DistanceFromSource >= Config.Calculation.maxDistanceFromDepot)
                    {
                        Logger.Debug(String.Format("{0} is being ignored because it's distance from the depot of {1} is farther than the maximimum config distance of {2} miles", location.Account, location.DistanceFromSource, Config.Calculation.maxDistanceFromDepot));
                        continue;
                    }

                    possibleLocations.Add(location);
                }
                return possibleLocations;
        }

        public static Location FindNearestLocation(Location source, List<Location> locations)
        {
                if (source.nearestLocation != null)
                {
                    if (locations.Contains(source.nearestLocation))
                    {
                        Logger.Trace("Cached" + source.nearestLocation.Address + ": is " + source.distanceToNearestLocation + " miles from " + source.Address);
                        return source.nearestLocation;
                    }
                }

                if (locations.Count == 1)
                    return locations.First();

                double shortestDistance = double.MaxValue;
                Location nearestLocation = new Location();
                List<Location> LocationsExceptSource = new List<Location>(locations);
                LocationsExceptSource.Remove(source);

                foreach (Location location in LocationsExceptSource)
                {
                    double thisDistance = CalculateDistance(source, location);
                    if (thisDistance <= shortestDistance)
                    {
                        nearestLocation = location;
                        shortestDistance = thisDistance;
                    }
                    Logger.Trace(nearestLocation.Address + ": is " + shortestDistance + " miles from " + source.Address);
                }
                source.nearestLocation = nearestLocation;
                if (source.distanceToNearestLocation == null)
                    source.distanceToNearestLocation = CalculateDistance(source, nearestLocation);
                return nearestLocation;
        }


        private class NeighborsDistance
        {
            public Location neighbor;
            public double distance;
        }

        public static List<Location> FindNeighbors(Location source, List<Location> locations, uint neighborCount = 50)
        {
                List<NeighborsDistance> neighborsDistance = new List<NeighborsDistance>();

                foreach (Location location in locations)
                {
                    double thisDistance = CalculateDistance(source, location);
                    NeighborsDistance thisNeighborDistance = new NeighborsDistance
                    {
                        neighbor = location,
                        distance = thisDistance
                    };

                    //if (thisNeighborDistance.distance <= 50)

                    neighborsDistance.Add(thisNeighborDistance);
                }
                //sort the neighbors by distance property asc
                lock (addLock)
                    neighborsDistance.Sort((x, y) => x.distance.CompareTo(y.distance));
                List<Location> neighbors = neighborsDistance.Select(a => a.neighbor).Take((int)neighborCount).ToList();
                return neighbors;
        }

        public List<Location> GetHighestPrioritylocations(List<Location> locations, int count)
        {
                locations.Sort((a, b) => a.DaysUntilDue.Value.CompareTo(b.DaysUntilDue.Value));
                List<Location> highestPrioritylocations = locations.Take(count).ToList();

                Logger.Trace("Got highest priority locations: " + highestPrioritylocations.ToList().ToString());
                return highestPrioritylocations;
        }


        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        private TimeSpan CalculateTravelTime(double distanceMiles)
        {
                double travelTimeMinutes = 0;
                double cityRadius = 5;
                //distance of less than n miles is considered to be within city, since very close locations will not involve highway mileage.
                //Allow for that same ammount of miles to get on the highway if the distance is greater
                //This is a very simple heuristic that assumes distances as the crow flies
                if (distanceMiles < cityRadius)
                {
                    travelTimeMinutes = distanceMiles * (60 / Config.Calculation.averageCityTravelSpeed);
                }
                else
                {
                    travelTimeMinutes += cityRadius * (60 / Config.Calculation.averageCityTravelSpeed);
                    travelTimeMinutes += (distanceMiles - cityRadius) * (60 / Config.Calculation.averageHighwayTravelSpeed);
                }

                if (travelTimeMinutes < 0)
                    throw new Exception("Travel time cannot be negative");

                TimeSpan travelTime = TimeSpan.FromMinutes(travelTimeMinutes);
                return travelTime;
        }

        public static double CalculateDistanceHaverSine(Location l1, Location l2)
        {
            var R = 3963.190592; // Earth’s mean radius in miles
            var dLat = Radians(l2.Coordinates.Lat.Value - l1.Coordinates.Lat.Value);
            var dLong = Radians(l2.Coordinates.Lng.Value - l1.Coordinates.Lng.Value);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(Radians(l1.Coordinates.Lat.Value)) * Math.Cos(Radians(l2.Coordinates.Lat.Value)) *
              Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            return d; // returns the distance in miles
        }

        public static double CalculateDistance(Location l1, Location l2)
        {
                if (l1.CartesianCoordinates.X == null || l1.CartesianCoordinates.Y == null || l1.CartesianCoordinates.Z == null)
                    throw new Exception(String.Format("Attempting to calculate on null coordinates of a location with address {0}.  This will result in an error.", l1.Address));

                if (l2.CartesianCoordinates.Y == null || l2.CartesianCoordinates.Y == null || l2.CartesianCoordinates.Z == null)
                    throw new Exception(String.Format("Attempting to calculate on null coordinates of a location with address {0}.  This will result in an error.", l2.Address));
                return Math.Sqrt(Math.Pow(l2.CartesianCoordinates.X.Value - l1.CartesianCoordinates.X.Value, 2) + Math.Pow(l2.CartesianCoordinates.Y.Value - l1.CartesianCoordinates.Y.Value, 2) + Math.Pow(l2.CartesianCoordinates.Z.Value - l1.CartesianCoordinates.Z.Value, 2));
        }
    }
}