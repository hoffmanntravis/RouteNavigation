using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Device.Location;
using System.Threading;
using NLog;
using System.Diagnostics;

namespace RouteNavigation
{
    public class RouteCalculator
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public Metadata metadata = new Metadata();
        public List<Route> routes = new List<Route>();
        public Location origin = new Location();
        public int neighborCount = 60;
        public Guid activityId;
        protected Config config;
        protected List<Location> allLocations;
        protected List<Vehicle> allVehicles;
        protected double localRadiusDivisor = 50;

        public List<Location> orphanedLocations = new List<Location>();
        static Object calcLock = new Object();
        protected int routePosition = 0;

        //protected double matrixDistanceFromSourceMultiplier = 1;

        protected DateTime startDate = (System.DateTime.Now.Date).AddDays(1);

        public RouteCalculator(Config c, List<Location> locations, List<Vehicle> vehicles)
        {
            config = c;
            allLocations = new List<Location>(locations);
            allVehicles = new List<Vehicle>(vehicles);
            origin = Calculation.origin;
            //remove the origin from all locations since it's only there for routing purposes and is not part of the set we are interested in
            allLocations.RemoveAll(s => s.address == origin.address);
        }

        public RouteCalculator()
        {
            config = DataAccess.GetConfig();
            allLocations = new List<Location>(DataAccess.GetLocations());
            allVehicles = new List<Vehicle>(DataAccess.GetVehicles());

            origin = Calculation.origin;
            //remove the origin from all locations since it's only there for routing purposes and is not part of the set we are interested in
            if (!(origin is null))
            {
                allLocations.RemoveAll(s => s.address == origin.address);
            }
        }

        public void CalculateRoutes(List<Location> locations)
        {
            //remove the origin from all locations since it's only there for routing purposes and is not part of the set we are interested in
            locations.RemoveAll(s => s.address == origin.address);
            List<Vehicle> availableVehicles = allVehicles.Where(a => a.operational == true).ToList();
            routes = CalculateRoutes(locations, availableVehicles, startDate, origin, metadata);
        }

        public List<Route> CalculateRoutes(List<Location> availableLocations, List<Vehicle> availableVehicles, DateTime startDate, Location origin, Metadata metadata)
        {
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            activityId = Trace.CorrelationManager.ActivityId;
            DateTime startTime = config.Calculation.workdayStartTime;
            DateTime endTime = config.Calculation.workdayEndTime;
            try
            {
                availableLocations = GetPossibleLocations(availableVehicles, availableLocations);

                List<Location> longOverDueLocations = availableLocations.Where(a => a.daysUntilDue <= (config.Calculation.maximumDaysOverdue * -1) && a.lastVisited != default(DateTime)).ToList();
                availableLocations = availableLocations.Except(longOverDueLocations).ToList();
                availableLocations = availableLocations.Except(availableLocations.Where(a => a.coordinates.lat is double.NaN || a.coordinates.lng is double.NaN)).ToList();

                if (origin == null)
                {
                    Exception exception = new Exception("Origin is null.  Please set it in the config page, or calculation will fail.");
                    Logger.Error(exception.ToString());
                    throw exception;
                }

                metadata.intakeLocations.AddRange(availableLocations);
                //Attempting to move line below to genetic algorithm
                //availableLocations.ForEach(a => a.neighbors = FindNeighbors(a, availableLocations, neighborCount));

                //Remove any locations that would be picked up too soon to be relevent.  We'll invoke a recursive call at the end to deal with these.
                List<Location> laterDateLocations = GetLaterDateLocations(availableLocations);

                //UpdateDistanceFromSource(allLocations);
                //UpdateMatrixWeight(allLocations);

                //sort the locations by distance from the source in descending order
                //availableLocations.Sort((a, b) => b.distanceFromSource.CompareTo(a.distanceFromSource));

                //build routes until all locations are exhausted


                while (availableLocations.Count > 0)
                {
                    if (availableVehicles.Count == 0)
                    {
                        //get some more vehicles and start a new day, with new routes
                        startDate = startDate.AddDays(1);
                        //logic to discard weekends for route days
                        if (startDate.DayOfWeek == DayOfWeek.Saturday)
                            startDate = startDate.AddDays(2);
                        if (startDate.DayOfWeek == DayOfWeek.Sunday)
                            startDate = startDate.AddDays(1);
                        availableVehicles = allVehicles.Where(a => a.operational == true).ToList();
                    }

                    //sort vehicles by size descending.  We do this to ensure that large vehicles are handled first since they have a limited location list available to them.
                    availableVehicles.Sort((a, b) => b.physicalSize.CompareTo(a.physicalSize));
                    Vehicle vehicle = availableVehicles.First();

                    List<Location> compatibleLocations = new List<Location>();
                    compatibleLocations = GetCompatibleLocations(vehicle, availableLocations.ToList());
                    //Find the highest priority location that the truck can serve
                    //List<Location> highestPriorityLocations = GetHighestPrioritylocations(compatibleLocations, 1);

                    Route potentialRoute = new Route(origin);
                    
                    DateTime currentTime = startTime;
                    potentialRoute.allLocations.Add(origin);

                    Location previousLocation = origin;
                    while (compatibleLocations.Count > 0)
                    {
                        DateTime potentialTime = currentTime;
                        //get the nearest location in the list of compatible locations based on distance algorithm (lat / lng)
                        //Location nearestLocation = FindNearestLocation(searchStart, compatibleLocations);
                        Location nextLocation = compatibleLocations.First();

                        nextLocation.currentGallonsEstimate = EstimateLocationGallons(nextLocation);

                        if (config.Features.vehicleFillLevel == true)
                        {
                            if (CheckVehicleCanAcceptMoreLiquid(vehicle, nextLocation))
                            {
                                compatibleLocations.Remove(nextLocation);
                                continue;
                            }
                        }
                        double nextLocationDistanceMiles = CalculateDistance(previousLocation, nextLocation);
                        double distanceToDepot = CalculateDistance(nextLocation, origin);
                        TimeSpan travelTime = CalculateTravelTime(nextLocationDistanceMiles);
                        potentialTime += travelTime;


                        //If the location is not allowed before or after a certain time and the potential time has been exceeded, remove it.  Calc will advance a day and deal with it at that point if it's not compatible currently.

                        if (nextLocation.pickupWindowStartTime != DateTime.MinValue)
                            if (potentialTime < nextLocation.pickupWindowStartTime)
                                compatibleLocations.Remove(nextLocation);

                        if (nextLocation.pickupWindowEndTime != DateTime.MinValue)
                            if (potentialTime > nextLocation.pickupWindowEndTime)
                                compatibleLocations.Remove(nextLocation);

                        if (nextLocation.type == "oil")
                        {
                            potentialTime += TimeSpan.FromMinutes(config.Calculation.oilPickupAverageDurationMinutes);
                        }
                        if (nextLocation.type == "grease")
                        {
                            potentialTime += TimeSpan.FromMinutes(config.Calculation.greasePickupAverageDurationMinutes);
                        }

                        //get the current total distance, including the trip back to the depot for comparison to max distance setting

                        potentialRoute.distanceMiles = calculateTotalDistance(potentialRoute.allLocations) + nextLocationDistanceMiles;
                        Logger.Trace(string.Format("potential route distance is {0} compared to a threshold of {1}", potentialRoute.distanceMiles, config.Calculation.routeDistanceMaxMiles));

                        if (potentialRoute.distanceMiles is Double.NaN)
                        {
                            Logger.Error(String.Format("Locations are {0} and {1} with gps coordinates of {2}:{3} and {4}:{5}", origin, nextLocation, origin.coordinates.lat, origin.coordinates.lng, nextLocation.coordinates.lat, nextLocation.coordinates.lng));
                            Logger.Error("potentialRoute.distanceMiles is Double.NaN");
                        }

                        double localRadiusTolerance = distanceToDepot / localRadiusDivisor;
                        //This is only relevent if we have a location in the route.  Otherwise, we may end up with no valid locations.  
                        if (potentialRoute.allLocations.Count > 0)
                        {
                            //if the location is within a certain radius, even if it means the day length being exceeded
                            if (potentialTime > endTime)
                            {
                                    Logger.Trace(String.Format("Removing location {0}.  Adding this location would put the route time at {1} which is later than {2}", nextLocation.locationName, potentialTime, endTime));
                                    compatibleLocations.Remove(nextLocation);
                                    continue;
                            }

                            if (potentialRoute.distanceMiles > config.Calculation.routeDistanceMaxMiles)
                            {
                                //if the location is within a certain radius, visit anyway even if it exceeds the total mileage
                                if (nextLocationDistanceMiles < localRadiusTolerance)
                                {
                                    Logger.Trace(String.Format("Distance from {1} to {0} is within 1/{2} of the distance back to the depot ({3} miles compared to {4} miles).  Will not remove from compatible locations.", nextLocation.locationName, previousLocation.locationName, localRadiusDivisor, nextLocationDistanceMiles, localRadiusTolerance));
                                }
                                else
                                {
                                    Logger.Trace(String.Format("Removing location {0}.  Distance from {1} to {0} is not within 1/{2} of the distance back to the depot ({3} miles compared to {4} miles).  Additionally, {5} is greater than the maximum route distance of {6} miles", nextLocation.locationName, previousLocation.locationName, localRadiusDivisor, nextLocationDistanceMiles, localRadiusTolerance, potentialRoute.distanceMiles, config.Calculation.routeDistanceMaxMiles));
                                    compatibleLocations.Remove(nextLocation);
                                    continue;
                                }
                            }
                        }

                        //Made it past any checks that would preclude this nearest route from getting added, add it as a waypoint on the route
                        vehicle.currentGallons += nextLocation.currentGallonsEstimate;

                        //add in the average visit time
                        potentialRoute.waypoints.Add(nextLocation);
                        potentialRoute.allLocations.Add(nextLocation);
                        availableLocations.Remove(nextLocation);
                        compatibleLocations.Remove(nextLocation);
                        //searchStart = nextLocation;
                        currentTime = potentialTime;
                        previousLocation = nextLocation;
                    }

                    //Add the time to travel back to the depot
                    double distanceToDepot2 = CalculateDistance(previousLocation, origin);
                    TimeSpan travelTime2 = CalculateTravelTime(distanceToDepot2);
                    currentTime.Add(travelTime2);

                    potentialRoute.allLocations.Add(origin);

                    /*//override nearest location with locations along the route if they are within five miles
                    foreach (Location apiLoc in apiRoute.waypoints)
                    {
                        List<Location> swapCandidates = new List<Location>(potentialRoute.waypoints);
                        Location swapCandidate = FindFarthestLocation(origin,potentialRoute.waypoints);

                            if (CalculateDistance(FindNearestLocation(apiLoc, compatibleLocations), searchStart) <= 5)
                            {
                                nearestLocation = apiLoc;
                            }
                    }
                    */

                    potentialRoute.assignedVehicle = vehicle;
                    potentialRoute.waypoints.ForEach(r => r.assignedVehicle = vehicle);
                    availableVehicles.Remove(vehicle);
                    potentialRoute.date = startDate;
                    potentialRoute = calculateTSPRouteNN(potentialRoute);
                    //potentialRoute = calculateTSPRouteTwoOpt(potentialRoute);
                    potentialRoute.distanceMiles = calculateTotalDistance(potentialRoute.allLocations);
                    potentialRoute.totalTime = currentTime - startTime;
                    //int oilLocationsCount = potentialRoute.allLocations.Where(a => a.type == "oil").ToList().Count;
                    //int greaseLocationsCount = potentialRoute.allLocations.Where(a => a.type == "grease").ToList().Count;
                    //Logger.Log(String.Format("there are {0} oil locations and {1} grease locations.", oilLocationsCount, greaseLocationsCount), "DEBUG");
                    potentialRoute.averageLocationDistance = calculateAverageLocationDistance(potentialRoute);
                    Logger.Trace("TSP calculated a shortest route 'flight' distance of " + potentialRoute.distanceMiles);
                    routes.Add(potentialRoute);
                }

                if (laterDateLocations.Count > 0)
                {
                    //Get locations that are too soon to handle now.  Then, sort them by the last time they were visited.
                    laterDateLocations.Sort((a, b) => a.daysUntilDue.CompareTo(b.daysUntilDue));
                    Location nextNearestLocationByDate = laterDateLocations.First();
                    //subtract the last vistited date and minimum days until pickup from the intended start date and convert to an integer days.  
                    //This will make the recursive algorithm efficient and tell it what day to start searching on again to create a future route that is compatible with our minimum pickup interval.
                    double daysToAdd = (nextNearestLocationByDate.pickupIntervalDays - (startDate - nextNearestLocationByDate.lastVisited).TotalDays) - config.Calculation.minimDaysUntilPickup;
                    availableVehicles = allVehicles;
                    startDate = startDate.AddDays(daysToAdd);
                    CalculateRoutes(laterDateLocations, availableVehicles, startDate, origin, metadata);
                }

                foreach (Route route in routes)
                {
                    /* potential for additional metadata based on api information
                     * 
                     * foreach (ApiRoute.Leg leg in potentialApiRoute.googleProperties.Legs)
                        potentialRoute.totalTime += potentialRoute.totalTime.Add(TimeSpan.FromSeconds(leg.Duration.Value));

                    //add the distance of each leg to the total distance of the route.  Api reports back in meters, so convert to miles.
                    foreach (ApiRoute.Leg leg in potentialApiRoute.googleProperties.Legs)
                        potentialRoute.distanceMiles += leg.Distance.Value / 1609.34;
                    */

                    //calculate metadata on waypoints since origin does not need visiting
                    metadata.processedLocations.AddRange(route.waypoints);
                    metadata.routesDuration += route.totalTime;
                    metadata.routesLengthMiles += route.distanceMiles;

                    if (metadata.routesLengthMiles is Double.NaN)
                    {
                        Logger.Error("metadata.routesLengthMiles is Double.NaN");
                    }
                }

                if (routes.Count > 0)
                {
                    metadata.averageRouteDistanceMiles = calculateAverageRouteDistance(routes);
                    metadata.averageRouteDistanceStdDev = calculateRoutesStdDev(routes);

                    //metadata.locationsHash = (this.metadata.processedLocations).GetHashCode();
                }

                else
                {
                    Logger.Error("Unable to create any routes.");
                }

                metadata.orphanedLocations = allLocations.Where(x => !metadata.processedLocations.Any(y => y.address == x.address)).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            return routes;
        }

        public static Location FindFarthestLocation(Location source, List<Location> locations)
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

        public static double calculateTotalDistance(List<Location> locations)
        {
            double totalDistance = 0;
            for (int x = 0; x < locations.Count - 1; x++)
            {
                totalDistance += CalculateDistance(locations[x], locations[x + 1]);
            }
            return totalDistance;
        }

        protected List<Location> ThreeOptSwap(List<Location> route)
        {
            double previousBestDistance;
            double bestDistance;
            int iterations = 0;
            int routeHashStart = generateRouteHash(route);
            do
            {
                //add the depot back in to ensure the route is shortest with the depot included
                previousBestDistance = calculateTotalDistance(route) + CalculateDistance(route.Last(), origin);
                bestDistance = double.MaxValue;
                for (int i = 0; i < route.Count - 1; i++)
                {
                    for (int j = i; j < route.Count - 1; j++)
                    {
                        for (int k = j; k < route.Count; k++)
                        {
                            List<Location> newRoute = runThreeOptSwap(route, i, j, k);
                            if (routeHashStart != generateRouteHash(newRoute))
                                throw new Exception("hashes do not match!");
                            double newDistance = calculateTotalDistance(newRoute) + CalculateDistance(newRoute.Last(), origin);
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

        protected List<Location> TwoOptSwap(List<Location> route)
        {
            double previousBestDistance;
            double bestDistance;
            int iterations = 0;
            int routeHashStart = generateRouteHash(route);
            do
            {
                //add the depot back in to ensure the route is shortest with the depot included
                previousBestDistance = calculateTotalDistance(route) + CalculateDistance(route.Last(), origin);
                bestDistance = double.MaxValue;
                for (int i = 0; i < route.Count - 1; i++)
                {
                    for (int j = i; j < route.Count; j++)
                    {
                        List<Location> newRoute = RunTwoOptSwap(route, i, j);
                        if (routeHashStart != generateRouteHash(newRoute))
                            throw new Exception("hashes do not match!");
                        double newDistance = calculateTotalDistance(newRoute) + CalculateDistance(newRoute.Last(), origin);
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
            Logger.Info("Ran " + iterations + " iterations of TwoOpt TSP");
            return route;
        }

        public static List<Location> runThreeOptSwap(List<Location> locations, int i, int j, int k)
        {
            List<Location> newRoute = new List<Location>();
            for (int x = 0; x <= i - 1; x++)
            {
                newRoute.Add(locations[x]);
            }

            List<Location> reverseLocations = new List<Location>();
            for (int x = i; x <= j - 1; x++)
            {
                reverseLocations.Add(locations[x]);
            }
            reverseLocations.Reverse();
            newRoute.AddRange(reverseLocations);

            reverseLocations = new List<Location>();
            for (int x = j; x <= k; x++)
            {
                reverseLocations.Add(locations[x]);
            }
            reverseLocations.Reverse();
            newRoute.AddRange(reverseLocations);

            for (int x = k + 1; x < locations.Count; x++)
            {
                newRoute.Add(locations[x]);
            }

            return newRoute;
        }

        public static List<Location> RunTwoOptSwap(List<Location> locations, int i, int j)
        {
            List<Location> newRoute = new List<Location>();
            for (int x = 0; x <= i - 1; x++)
            {
                newRoute.Add(locations[x]);
            }

            List<Location> reverseLocations = new List<Location>();
            for (int x = i; x <= j; x++)
            {
                reverseLocations.Add(locations[x]);
            }
            reverseLocations.Reverse();
            newRoute.AddRange(reverseLocations);

            for (int x = j + 1; x < locations.Count; x++)
            {
                newRoute.Add(locations[x]);
            }

            return newRoute;
        }

        protected static int generateRouteHash(List<Location> locations)
        {
            int hash = 0;
            try
            {
                List<Location> locationsCopy = new List<Location>(locations);
                string concat = "";
                locationsCopy.Sort((a, b) => a.address.CompareTo(b.address));
                foreach (Location location in locationsCopy)
                {
                    concat += location.address;
                }

                hash = concat.GetHashCode();
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
            return hash;
        }

        public static Route calculateTSPRouteNN(Route route)
        {
            try
            {
                Logger.Trace("attempting to TSP. Rearranging locations...");
                route.waypoints = nearestNeighbor(route.waypoints);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
            return route;
        }

        public Route calculateTSPRouteTwoOpt(Route route)
        {
            try
            {
                //Logger.Log("attempting to TSP. Rearranging locations...");
                route.waypoints = TwoOptSwap(route.waypoints);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
            return route;
        }

        public Route calculateTSPRouteThreeOpt(Route route)
        {
            try
            {
                //Logger.Log("attempting to TSP. Rearranging locations...");
                route.waypoints = ThreeOptSwap(route.waypoints);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
            return route;
        }

        protected static List<Location> nearestNeighbor(List<Location> route)
        {
            //int routeHashStart = generateRouteHash(route);

            List<Location> nearestNeighborRoute = new List<Location>();
            List<Location> unVisitedNodes = new List<Location>(route);

            Location nearest = route.First();

            foreach (Location location in route)
            {
                nearest = FindNearestLocation(nearest, unVisitedNodes);
                nearestNeighborRoute.Add(nearest);
                unVisitedNodes.Remove(nearest);
            }
            //if (routeHashStart != generateRouteHash(nearestNeighborRoute))
            //    throw new Exception("hashes do not match!");

            route = nearestNeighborRoute;

            return route;
        }

        public static double calculateAverageRouteDistance(List<Route> routes)
        {
            double average = 0;
            double totalDistance = 0;
            foreach (Route route in routes)
            {
                totalDistance += route.distanceMiles;
            }

            average = totalDistance / routes.Count;
            return average;
        }

        public static double calculateAverageLocationDistance(Route route)
        {
            double average = route.distanceMiles / route.allLocations.Count;
            return average;
        }

        private double calculateRoutesStdDev(List<Route> routes)
        {
            List<double> values = new List<double>();

            foreach (Route route in routes)
            {
                values.Add(route.distanceMiles);
            }

            double avg = calculateAverageRouteDistance(routes);
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        protected bool CheckVehicleCanAcceptMoreLiquid(Vehicle vehicle, Location location)
        {
            //Check if the vehicle can accept more gallons.  Also, multiple the total gallons by a percentage.  Finally, check that the vehicle isn't empty, otherwise we're going to visit regadless.
            if (vehicle.currentGallons + location.currentGallonsEstimate >= vehicle.capacityGallons * ((100 - config.Calculation.currentFillLevelErrorMarginPercent) / 100) && vehicle.currentGallons != 0)
            {
                return false;
            }
            return true;
        }

        protected double EstimateLocationGallons(Location location)
        {
            double currentGallonsEstimate;
            if (location.daysUntilDue > 0)
                currentGallonsEstimate = (location.daysUntilDue / location.pickupIntervalDays) * location.capacityGallons;
            else
                //capacity is assumed to be full if we have lapsed since the last visit
                currentGallonsEstimate = location.capacityGallons;

            return currentGallonsEstimate;
        }

        protected List<Location> GetLaterDateLocations(List<Location> availableLocations)
        {
            List<Location> laterDateLocations = new List<Location>();
            foreach (Location l in availableLocations)
            {
                double startDateDaysUntilDue = l.pickupIntervalDays - (startDate - l.lastVisited).TotalDays;
                if (startDateDaysUntilDue > config.Calculation.minimDaysUntilPickup)
                {
                    laterDateLocations.Add(l);
                }
            }

            foreach (Location l in laterDateLocations)
            {
                availableLocations.Remove(l);
            }
            return laterDateLocations;
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
        }

        protected List<Location> GetCompatibleLocations(Vehicle vehicle, List<Location> locations)
        {
            List<Location> compatibleLocations = new List<Location>();
            foreach (Location location in locations)
            {
                if (vehicle.physicalSize <= location.vehicleSize)
                {
                    compatibleLocations.Add(location);
                    continue;
                }
            }
            return compatibleLocations;
        }

        public bool checkVehicleLocationCompatibility(Vehicle vehicle, Location location)
        {
            if (vehicle.physicalSize <= location.vehicleSize)
                return true;
            else return false;
        }

        protected List<Location> GetPossibleLocations(List<Vehicle> vehicles, List<Location> locations)
        {
            List<Location> possibleLocations = new List<Location>();
            foreach (Location location in locations)
            {
                foreach (Vehicle vehicle in vehicles)
                {
                    if (vehicle.physicalSize <= location.vehicleSize)
                    {
                        //if we find a vehicle that works with the location, add the location to the list of possible locations and break out to the next location
                        possibleLocations.Add(location);
                        break;
                    }
                }

            }
            return possibleLocations;
        }

        public static Location FindNearestLocation(Location source, List<Location> locations)
        {
            double shortestDistance = double.MaxValue;
            Location nearestLocation = new Location();
            foreach (Location location in locations)
            {
                double thisDistance = CalculateDistance(source, location);
                if (thisDistance <= shortestDistance)
                {
                    nearestLocation = location;
                    shortestDistance = thisDistance;
                    Logger.Trace(nearestLocation.address + ": is " + shortestDistance + " miles from " + source.address);
                }
            }
            return nearestLocation;
        }

        private class NeighborsDistance
        {
            public Location neighbor;
            public double distance;
        }

        public static List<Location> FindNeighbors(Location source, List<Location> locations, int neighborCount = 50)
        {
            List<NeighborsDistance> neighborsDistance = new List<NeighborsDistance>();

            foreach (Location location in locations)
            {
                double thisDistance = CalculateDistance(source, location);
                NeighborsDistance thisNeighborDistance = new NeighborsDistance();
                thisNeighborDistance.neighbor = location;
                thisNeighborDistance.distance = thisDistance;
                //if (thisNeighborDistance.distance <= 50)
                neighborsDistance.Add(thisNeighborDistance);
            }
            //sort the neighbors by distance property asc
            neighborsDistance.Sort((x, y) => x.distance.CompareTo(y.distance));

            //make sure we don't attempt to take more neighbors than fit inside the search radius
            //neighborCount = neighborsDistance.Count;

            // take the first 'neighborCount' (n) of just the neighbors, not the distance, and convert to list for return
            List<Location> neighbors = neighborsDistance.Select(a => a.neighbor).Take(neighborCount).ToList();
            return neighbors;
        }

        public static List<Location> GetHighestPrioritylocations(List<Location> locations, int guaranteedVisitedlocationsCount)
        {
            locations.Sort((a, b) => b.matrixWeight.CompareTo(a.matrixWeight));
            List<Location> highestPrioritylocations = locations.Take(guaranteedVisitedlocationsCount).ToList();
            Logger.Trace("Got highest priority locations: " + highestPrioritylocations.ToList().ToString());
            return highestPrioritylocations;
        }

        public void UpdateDistanceFromSource(List<Location> locations)
        {
            try
            {
                DataAccess.UpdateDistanceFromSource(locations);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to append distanceFromSource data for the RouteCalculator Class.");
                Logger.Error(exception.ToString());
            }
        }

        public void UpdateMatrixWeight(List<Location> locations)
        {
            try
            {
                foreach (Location location in locations)
                {
                    location.matrixWeight = CalculateWeight(location);
                }
                DataAccess.UpdateMatrixWeight(locations);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to append matrixWeight data for the RouteCalculator Class.");
                Logger.Error(exception.ToString());
            }
        }

        public double CalculateWeight(Location location)
        {
            double algPriority = config.matrix.priorityMultiplier * location.clientPriority;
            double algDaysUntilDue = -1 * (Math.Sign(location.daysUntilDue) * (Math.Pow(Math.Abs(location.daysUntilDue), config.matrix.daysUntilDueExponent)));
            //If the account is overdue, increase the value
            if (location.daysUntilDue <= 0)
            {
                algDaysUntilDue = algDaysUntilDue * config.matrix.overDueMultiplier;
            }
            //Theoretically can prioritize nearby locations.  However, since we have to eventually visit all locations, this doesn't seem very advantageous
            double algDistance;
            if (config.Features.prioritizeNearestLocation == true)
                algDistance = (config.matrix.distanceFromSourceMultiplier * location.distanceFromSource);
            else
                algDistance = 0;

            double weight = algPriority + algDaysUntilDue + algDistance;
            return weight;
        }

        protected static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        protected TimeSpan CalculateTravelTime(double distanceMiles)
        {
            double travelTimeMinutes = 0;
            double cityRadius = 5;
            //distance of less than n miles is considered to be within city, since very close locations will not involve highway mileage.
            //Allow for that same ammount of miles to get on the highway if the distance is greater
            //This is a very simple heuristic that assumes distances as the crow flies
            if (distanceMiles < cityRadius)
            {
                travelTimeMinutes = distanceMiles * (60 / config.Calculation.averageCityTravelSpeed);
            }
            else
            {
                travelTimeMinutes += cityRadius * (60 / config.Calculation.averageCityTravelSpeed);
                travelTimeMinutes += (distanceMiles - cityRadius) * (60 / config.Calculation.averageHighwayTravelSpeed);
            }

            TimeSpan travelTime = TimeSpan.FromMinutes(travelTimeMinutes);
            return travelTime;
        }

        protected static double CalculateDistance(Location p1, Location p2)
        {
            var R = 3963.190592; // Earth’s mean radius in miles
            var dLat = Radians(p2.coordinates.lat - p1.coordinates.lat);
            var dLong = Radians(p2.coordinates.lng - p1.coordinates.lng);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(Radians(p1.coordinates.lat)) * Math.Cos(Radians(p2.coordinates.lat)) *
              Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d; // returns the distance in miles
        }
    }
}