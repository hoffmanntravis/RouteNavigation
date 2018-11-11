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
        public Location origin = Config.Calculation.origin;
        public uint neighborCount = 60;
        public Guid activityId;
        private List<Location> allLocations;
        private List<Vehicle> availableVehicles;

        public List<Location> orphanedLocations = new List<Location>();

        private DateTime startDate = AdvanceDateToNextWeekday(System.DateTime.Now.Date);


        public RouteCalculator(List<Location> allLocations, List<Vehicle> availableVehicles)
        {
            this.allLocations = new List<Location>(allLocations);
            this.availableVehicles = new List<Vehicle>(availableVehicles);
        }

        public List<Route> CalculateRoutes(List<Location> availableLocations)
        {
            List<Vehicle> currentVehicles = availableVehicles.ToList();
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            activityId = Trace.CorrelationManager.ActivityId;
            DateTime startTime = Config.Calculation.workdayStartTime;
            DateTime endTime = Config.Calculation.workdayEndTime;
            metadata.intakeLocations = allLocations;

            try
            {
                //This should get moved to the genetic algorithm as it is a static answer and recalculating is wasteful

                if (origin == null)
                {
                    Exception exception = new Exception("Origin is null.  Please set it in the config page, or calculation will fail.");
                    Logger.Error(exception);
                    throw exception;
                }

                List<Location> serviceNowLocations = GetRequireServiceNowLocations(availableLocations, startTime);
                availableLocations = availableLocations.Except(serviceNowLocations).ToList();
                availableLocations.InsertRange(0, serviceNowLocations);

                //UpdateDistanceFromSource(allLocations);

                //sort the locations by distance from the source in descending order
                //availableLocations.Sort((a, b) => b.distanceFromSource.CompareTo(a.distanceFromSource));

                //build routes until all locations are exhausted

                while (availableLocations.Count > 0)
                {
                    if (currentVehicles.Count == 0)
                    {
                        //get some more vehicles and start a new day, with new routes
                        currentVehicles = availableVehicles.ToList();
                        startDate = AdvanceDateToNextWeekday(startDate);
                    }
                    Logger.Trace("startdate is {0}", startDate);
                    //Remove any locations that would be picked up too soon to be relevent.  We'll invoke a recursive call at the end to deal with these.
                    List<Location> availableLocationsWithPostponedLocations = availableLocations.ToList();
                    List<Location> postPonedLocations = GetLaterDateLocations(availableLocations);
                    availableLocations = availableLocations.Except(postPonedLocations).ToList();

                    //If all that is left are locations that need to be processed later, advance the date accordingly
                    if (postPonedLocations.Count > 0 && availableLocations.Count == 0)
                    {
                        Location firstDueLocation = postPonedLocations.OrderBy(a => a.daysUntilDue).First();
                        double daysElapsed = (startDate - firstDueLocation.lastVisited).TotalDays;
                        double daysToAdd = Config.Calculation.minimDaysUntilPickup - daysElapsed;
                        currentVehicles = availableVehicles.ToList();

                        startDate = startDate.AddDays((uint)daysToAdd);
                        availableLocations = postPonedLocations.ToList();
                        if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                            startDate = AdvanceDateToNextWeekday(startDate);
                        continue;
                    }

                    serviceNowLocations = GetRequireServiceNowLocations(availableLocations, startTime);
                    availableLocations = availableLocations.Except(serviceNowLocations).ToList();
                    availableLocations.InsertRange(0, serviceNowLocations);

                    //sort vehicles by size descending.  We do this to ensure that large vehicles are handled first since they have a limited location list available to them.
                    currentVehicles.Sort((a, b) => b.physicalSize.CompareTo(a.physicalSize));
                    Vehicle vehicle = currentVehicles.First();

                    List<Location> compatibleLocations = GetCompatibleLocations(vehicle, availableLocations.ToList());
                    //Find the highest priority location that the truck can serve
                    //List<Location> highestPriorityLocations = GetHighestPrioritylocations(compatibleLocations, 1);

                    Route potentialRoute = new Route();

                    DateTime currentTime = startTime;
                    double currentDistance = 0;
                    potentialRoute.allLocations.Add(origin);

                    Location previousLocation = origin;
                    while (compatibleLocations.Count > 0)
                    {
                        DateTime potentialTime = currentTime;
                        double potentialDistance = currentDistance;

                        serviceNowLocations = GetRequireServiceNowLocations(compatibleLocations, currentTime);
                        compatibleLocations = compatibleLocations.Except(serviceNowLocations).ToList();
                        compatibleLocations.InsertRange(0, serviceNowLocations);

                        Location nextLocation;
                        nextLocation = compatibleLocations.First();
                        
                        if (compatibleLocations.Count > 1)
                        {
                            Location nearestLocation = FindNearestLocation(previousLocation, compatibleLocations);
                            if (CalculateDistance(previousLocation, nearestLocation) == 0)
                                nextLocation = nearestLocation;
                        }

                        nextLocation.currentGallonsEstimate = EstimateLocationGallons(nextLocation);

                        if (Config.Features.vehicleFillLevel == true)
                        {
                            if (!(CheckVehicleCanAcceptMoreLiquid(vehicle, nextLocation)))
                            {
                                Logger.Trace(String.Format("Performing a dropoff.  This will take {0} minutes.  Resetting current gallons to 0.", Config.Calculation.dropOffTime));
                                vehicle.currentGallons = 0;
                                potentialTime.Add(Config.Calculation.dropOffTime);
                            }
                        }

                        double nextLocationDistanceMiles = CalculateDistance(previousLocation, nextLocation);

                        double distanceTolerance = nextLocation.distanceFromDepot * (Config.Calculation.searchRadiusFraction);

                        if (potentialRoute.waypoints.Count > 0)
                        {
                            if (nextLocationDistanceMiles >= Math.Max(distanceTolerance, Config.Calculation.searchMinimumDistance))
                            {
                                Logger.Trace(String.Format("Removing location {1} from compatible locations. Distance from {1} to {0} is greater than the radius search tolerance of {2} miles.", nextLocation.locationName, previousLocation.locationName, distanceTolerance));
                                compatibleLocations.Remove(nextLocation);
                                continue;
                            }
                            else
                                Logger.Trace(String.Format("Distance from {1} to {0} is less than the radius search tolerance of {2} miles.  Will not remove from compatible locations.", nextLocation.locationName, previousLocation.locationName, distanceTolerance));
                        }
                        else
                            Logger.Trace(String.Format("Route currently has 0 locations.  Adding {0} to populate the route.", nextLocation));

                        TimeSpan travelTime = CalculateTravelTime(nextLocationDistanceMiles);
                        Logger.Trace(String.Format("Travel time from {0} ({1}) to next location {2} ({3}) is {4} minutes", previousLocation.locationName, previousLocation.address, nextLocation.locationName, nextLocation.address, travelTime.TotalMinutes));
                        potentialTime += travelTime;

                        //If the time it would take to travel to the location is greater than the allowed pickup window, go anyway.  Otherwise it will never get visited.

                        if (travelTime <= nextLocation.pickupWindowEndTime - nextLocation.pickupWindowStartTime)
                        {
                            //If the location is not allowed before or after a certain time and the potential time has been exceeded, remove it.  Calc will advance a day and deal with it at that point if it's not compatible currently.
                            if (nextLocation.pickupWindowStartTime != DateTime.MinValue)
                                if ((potentialTime < nextLocation.pickupWindowStartTime))
                                {
                                    compatibleLocations.Remove(nextLocation);
                                    continue;
                                }

                            if (nextLocation.pickupWindowEndTime != DateTime.MaxValue)
                                if ((potentialTime > nextLocation.pickupWindowEndTime))
                                {
                                    compatibleLocations.Remove(nextLocation);
                                    continue;
                                }
                        }

                        if (nextLocation.type == "oil")
                        {
                            potentialTime += TimeSpan.FromMinutes(Config.Calculation.oilPickupAverageDurationMinutes);
                        }
                        if (nextLocation.type == "grease")
                        {
                            potentialTime += TimeSpan.FromMinutes(Config.Calculation.greasePickupAverageDurationMinutes);
                        }

                        
                        //get the current total distance, including the trip back to the depot for comparison to max distance setting
                        potentialRoute.distanceMiles += nextLocationDistanceMiles;
                        //Logger.Trace(string.Format("potential route distance is {0} compared to a threshold of {1}", potentialRoute.distanceMiles, config.Calculation.routeDistanceMaxMiles));

                        if (potentialRoute.distanceMiles is Double.NaN)
                        {
                            Logger.Error(String.Format("Locations are {0} and {1} with gps coordinates of {2}:{3} and {4}:{5}", origin, nextLocation, origin.coordinates.lat, origin.coordinates.lng, nextLocation.coordinates.lat, nextLocation.coordinates.lng));
                            Logger.Error("potentialRoute.distanceMiles is Double.NaN");
                        }

                        //double localRadiusTolerance = nextLocation.distanceFromDepot / localRadiusDivisor;
                        //This is only relevent if we have a waypoint in the route.  Otherwise, we may end up with no valid locations.  
                        if (potentialRoute.waypoints.Count > 0)
                        {
                            //if the location is within a certain radius, even if it means the day length being exceeded
                            if (potentialTime > endTime)
                            {
                                Logger.Trace(String.Format("Removing location {0}.  Adding this location would put the route time at {1} which is later than {2}", nextLocation.locationName, potentialTime, endTime));
                                compatibleLocations.Remove(nextLocation);
                                continue;
                            }
                        }

                        //Made it past any checks that would preclude this nearest route from getting added, add it as a waypoint on the route
                        vehicle.currentGallons += nextLocation.currentGallonsEstimate;
                        nextLocation.intendedPickupDate = potentialTime;

                        //add in the average visit time
                        potentialRoute.waypoints.Add(nextLocation);
                        availableLocations.Remove(nextLocation);
                        compatibleLocations.Remove(nextLocation);
                        //searchStart = nextLocation;
                        currentTime = potentialTime;
                        
                        previousLocation = nextLocation;
                    }

                    //Add the time to travel back to the depot
                    double distanceToDepotFromLastWaypoint = CalculateDistance(previousLocation, origin);
                    TimeSpan travelTimeBackToDepot = CalculateTravelTime(distanceToDepotFromLastWaypoint);
                    Logger.Trace(String.Format("Travel time back from {0} ({1}) to {2} ({3}) is {4} minutes", previousLocation.locationName, previousLocation.address, origin.locationName, origin.address, travelTimeBackToDepot.TotalMinutes));
                    currentTime.Add(travelTimeBackToDepot);

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

                    /*
                    List<Location> serviceNowLocations = GetRequireServiceNowLocations(availableLocations);

                    //If every location is overdue, proceed as normal.  There is no point in attempting to rearrange routes.
                    if (availableLocations.Count == serviceNowLocations.Count)
                        serviceNowLocations.Clear();

                    Logger.Trace("There are {0} locations that will be moved to the front of the list and receive priority on projected date {1}", serviceNowLocations.Count, startDate);

                    List<Location> locationsExceptServiceNowLocations = potentialRoute.waypoints.Except(serviceNowLocations).ToList();
                    //new code re-arranges service now locations at the front of the list and uses whatever the genetic algorithm initially calculated otherwise

                    
                    for (int x = 0; x < Math.Min(serviceNowLocations.Count, potentialRoute.waypoints.Count); x++)
                    {
                        //get waypoints that aren't overdue in the route

                        if (locationsExceptServiceNowLocations.Count > 1)
                        {
                            //find the nearest location of a waypoint in the route to the overdue location
                            Location locationToReplace = FindNearestLocation(serviceNowLocations[x], locationsExceptServiceNowLocations);
                            int replacementLocationIndex = potentialRoute.waypoints.IndexOf(locationToReplace);
                            Logger.Trace("{0} needs service now or it will become overdue.  Swapping with {1} to fulfill this requirement.", serviceNowLocations[x], locationToReplace.locationName);
                            potentialRoute.waypoints[replacementLocationIndex] = serviceNowLocations[x];
                            int originalIndex = availableLocations.IndexOf(serviceNowLocations[x]);
                            availableLocations.Remove(serviceNowLocations[x]);
                            availableLocations.Insert(originalIndex, locationToReplace);

                        }
                    }
                    
                    */

                    potentialRoute.allLocations.AddRange(potentialRoute.waypoints);
                    potentialRoute.allLocations.Add(origin);

                    if (potentialRoute.waypoints.Count == 0)
                        throw new Exception("Route waypoints count is 0.  Something went wrong.");

                    potentialRoute.assignedVehicle = vehicle;
                    potentialRoute.waypoints.ForEach(r => r.assignedVehicle = vehicle);
                    currentVehicles.Remove(vehicle);
                    potentialRoute.date = startDate;
                    potentialRoute = CalculateTSPRouteNN(potentialRoute);
                    //potentialRoute = calculateTSPRouteTwoOpt(potentialRoute);

                    potentialRoute.totalTime = currentTime - startTime;
                    //int oilLocationsCount = potentialRoute.allLocations.Where(a => a.type == "oil").ToList().Count;
                    //int greaseLocationsCount = potentialRoute.allLocations.Where(a => a.type == "grease").ToList().Count;
                    //Logger.Log(String.Format("there are {0} oil locations and {1} grease locations.", oilLocationsCount, greaseLocationsCount), "DEBUG");

                    potentialRoute.distanceMiles = CalculateTotalDistance(potentialRoute.allLocations, true);
                    Logger.Trace("TSP calculated a shortest route 'flight' distance of " + potentialRoute.distanceMiles);

                    SetProjectedDaysOverdue(potentialRoute.waypoints);
                    //List<Location> overdueLocations = potentialRoute.waypoints.Where(p => p.projectedAmountOverdue.TotalDays > 0).ToList();
                    //overdueLocations.ForEach(o => potentialRoute.distanceMiles += Math.Pow(o.projectedAmountOverdue.TotalDays,5));

                    potentialRoute.averageLocationDistance = CalculateAverageLocationDistance(potentialRoute);

                    routes.Add(potentialRoute);

                    //Add later date locations back in, so once an eligible date becomes applicable they will be processed.  A copy is used to preserve the original order as much as possible minus locations that were assigned to a route.
                    availableLocations = availableLocationsWithPostponedLocations.Except(potentialRoute.waypoints).ToList();
                }

                /*
                if (laterDateLocations.Count > 0)
                {
                    
                    //Get locations that are too soon to handle now.  Then, sort them by the last time they were visited.

                    Location firstDueLocation = laterDateLocations.OrderBy(a => a.daysUntilDue).First();
                    //subtract the last vistited date and minimum days until pickup from the intended start date and convert to an integer days.  
                    //This will make the recursive algorithm efficient and tell it what day to start searching on again to create a future route that is compatible with our minimum pickup interval.

                    double daysElapsed = (startDate - firstDueLocation.lastVisited).TotalDays;
                    double daysToAdd = Config.Calculation.minimDaysUntilPickup - daysElapsed;
                    currentVehicles = availableVehicles.ToList();
                    startDate = startDate.AddDays(daysToAdd);

                    Logger.Trace("Adding {0} later date locations to available locations and recalculating", laterDateLocations.Count);
                    CalculateRoutes(laterDateLocations);
                }*/



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
                    metadata.averageRouteDistanceMiles = CalculateAverageRouteDistance(routes);
                    metadata.averageRouteDistanceStdDev = CalculateRoutesStdDev(routes);
                    metadata.fitnessScore = metadata.CalculateFitnessScore();
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
                Logger.Error(e);
            }

            return routes;
        }

        private static DateTime AdvanceDateToNextWeekday(DateTime date)
        {
            date = date.AddDays(1);
            //logic to discard weekends for route days
            if (date.DayOfWeek == DayOfWeek.Saturday)
                date = date.AddDays(2);
            if (date.DayOfWeek == DayOfWeek.Sunday)
                date = date.AddDays(1);
            return date;
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

        public static double CalculateTotalDistance(List<Location> locations,bool roundTrip = false)
        {
            double totalDistance = 0;

            for (int x = 0; x < locations.Count - 1; x++)
                totalDistance += CalculateDistance(locations[x], locations[x + 1]);

            if (roundTrip)
                totalDistance += CalculateDistance(locations[locations.Count - 1], locations[0]);

            return totalDistance;
        }

        public static List<Location> ThreeOptSwap(List<Location> route)
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

        public static List<Location> TwoOptSwap(List<Location> route)
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
                    for (int j = i; j < route.Count; j++)
                    {
                        List<Location> newRoute = RunTwoOptSwap(route, i, j);
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
                iterations++;
            }
            while (bestDistance < previousBestDistance);
            Logger.Debug("Ran " + iterations + " iterations of TwoOpt TSP");
            return route;
        }

        public static List<Location> RunThreeOptSwap(List<Location> locations, int i, int j, int k)
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

        private static int GenerateRouteHash(List<Location> locations)
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
                Logger.Error(exception);
            }
            return hash;
        }

        public static Route CalculateTSPRouteNN(Route route)
        {
            try
            {
                Logger.Trace("attempting to TSP. Rearranging locations...");
                route.waypoints = NearestNeighbor(route.waypoints);
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
                //Logger.Log("attempting to TSP. Rearranging locations...");
                route.waypoints = TwoOptSwap(route.waypoints);
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
                //Logger.Log("attempting to TSP. Rearranging locations...");
                route.waypoints = ThreeOptSwap(route.waypoints);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            return route;
        }

        public static List<Location> NearestNeighbor(List<Location> route)
        {
            if (route.Count == 1)
                return route;

            //int routeHashStart = GenerateRouteHash(route);

            List<Location> nearestNeighborRoute = new List<Location>();
            List<Location> unVisitedNodes = new List<Location>(route);

            Location nearest = unVisitedNodes.First();
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

        public static double CalculateAverageRouteDistance(List<Route> routes)
        {
            double average = 0;
            double totalDistance = 0;
            foreach (Route route in routes)
                totalDistance += route.distanceMiles;

            average = totalDistance / routes.Count;
            return average;
        }

        public static double CalculateAverageLocationDistance(Route route)
        {
            //remove one location, because origin and destination are the same
            double average = route.distanceMiles / (route.allLocations.Count - 1);
            return average;
        }

        private double CalculateRoutesStdDev(List<Route> routes)
        {
            List<double> values = new List<double>();

            foreach (Route route in routes)
                values.Add(route.distanceMiles);

            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        private bool CheckVehicleCanAcceptMoreLiquid(Vehicle vehicle, Location location)
        {
            //Check if the vehicle can accept more gallons.  Also, multiple the total gallons by a percentage.  Finally, check that the vehicle isn't empty, otherwise we're going to visit regadless.
            if (vehicle.currentGallons + location.currentGallonsEstimate > vehicle.capacityGallons * ((100 - Config.Calculation.currentFillLevelErrorMarginPercent) / 100) && vehicle.currentGallons != 0)
                return false;
            return true;
        }

        private double EstimateLocationGallons(Location location)
        {
            double currentGallonsEstimate;
            if (location.daysUntilDue > 0)
                currentGallonsEstimate = (location.daysUntilDue / location.pickupIntervalDays) * location.capacityGallons;
            else
                //capacity is assumed to be full if we have lapsed since the last visit
                currentGallonsEstimate = location.capacityGallons;

            return currentGallonsEstimate;
        }

        private List<Location> GetLaterDateLocations(List<Location> availableLocations)
        {
            List<Location> laterDateLocations = new List<Location>();
            foreach (Location l in availableLocations)
            {
                double daysElapsed = (startDate - l.lastVisited).TotalDays;
                if (daysElapsed < Config.Calculation.minimDaysUntilPickup)
                    laterDateLocations.Add(l);
            }
            return laterDateLocations;
        }

        private void SetProjectedDaysOverdue(List<Location> availableLocations)
        {
            availableLocations.ForEach(a => a.projectedAmountOverdue = (a.intendedPickupDate - a.lastVisited) - TimeSpan.FromDays(a.pickupIntervalDays));
        }

        private List<Location> GetRequireServiceNowLocations(List<Location> availableLocations, DateTime currentDate)
        {
            List<Location> serviceNowLocations = new List<Location>();
            foreach (Location l in availableLocations)
            {
                if (l.intendedPickupDate == null)
                    l.intendedPickupDate = currentDate;
                double daysElapsed = (l.intendedPickupDate - l.lastVisited).TotalDays;
                //Set interval days to interval days -1 so we can be 'on time' instead of late
                if (daysElapsed >= l.pickupIntervalDays - 1)
                    serviceNowLocations.Add(l);
            }
            return serviceNowLocations;
        }

        public class Metadata
        {
            //public double fitnessDistanceWeight = .5;
            //public double fitnessAverageDistanceWeight = 1;
            //public double fitnessDistributionWeight = 1.6;
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

        private static List<Location> GetCompatibleLocations(Vehicle vehicle, List<Location> locations)
        {
            List<Location> compatibleLocations = locations.Where(l => vehicle.physicalSize <= l.vehicleSize).ToList();
            return compatibleLocations;
        }

        public static List<Location> GetPossibleLocations(List<Vehicle> vehicles, List<Location> locations)
        {
            List<Location> possibleLocations = new List<Location>();
            double smallestVehicle = vehicles.Min(v => v.physicalSize);

            foreach (Location location in locations)
            {
                if (location.vehicleSize <= smallestVehicle)
                {
                    Logger.Debug(String.Format("{0} is being ignored because it's size of {1} is smaller than the smallest vehicle size of {2}", location.locationName, location.vehicleSize, smallestVehicle));
                    //if we find a vehicle that works with the location, add the location to the list of possible locations and break out to the next location
                    continue;
                }

                if (location.daysUntilDue <= (Config.Calculation.maximumDaysOverdue * -1) && location.lastVisited != default(DateTime))
                {
                    Logger.Debug(String.Format("{0} is being ignored because it's last visited date of {1} is more than {2} days overdue", location.locationName, location.lastVisited, Config.Calculation.maximumDaysOverdue));
                    continue;
                }

                if (location.distanceFromDepot >= Config.Calculation.maxDistanceFromDepot)
                {
                    Logger.Debug(String.Format("{0} is being ignored because it's distance from the depot of {1} is farther than the maximimum config distance of {2} miles", location.locationName, location.distanceFromDepot, Config.Calculation.maxDistanceFromDepot));
                    continue;
                }
                possibleLocations.Add(location);
            }
            return possibleLocations;
        }

        public static Location FindNearestLocation(Location source, List<Location> locations)
        {
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
            neighborsDistance.Sort((x, y) => x.distance.CompareTo(y.distance));

            //make sure we don't attempt to take more neighbors than fit inside the search radius
            //neighborCount = neighborsDistance.Count;

            // take the first 'neighborCount' (n) of just the neighbors, not the distance, and convert to list for return
            List<Location> neighbors = neighborsDistance.Select(a => a.neighbor).Take((int)neighborCount).ToList();
            return neighbors;
        }

        public static List<Location> GetHighestPrioritylocations(List<Location> locations, int count)
        {
            locations.Sort((a, b) => a.daysUntilDue.CompareTo(b.daysUntilDue));
            List<Location> highestPrioritylocations = locations.Take(count).ToList();
            Logger.Trace("Got highest priority locations: " + highestPrioritylocations.ToList().ToString());
            return highestPrioritylocations;
        }

        public static void UpdateDistanceFromSource(List<Location> locations)
        {
            try
            {
                DataAccess.UpdateDistanceFromSource(locations);
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to append distanceFromSource data for the RouteCalculator Class.");
                Logger.Error(exception);
            }
        }


        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        private static TimeSpan CalculateTravelTime(double distanceMiles)
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

            TimeSpan travelTime = TimeSpan.FromMinutes(travelTimeMinutes);
            return travelTime;
        }

        public static double CalculateDistance(Location p1, Location p2)
        {
            var R = 3963.190592; // Earth’s mean radius in miles
            var dLat = Radians(p2.coordinates.lat - p1.coordinates.lat);
            var dLong = Radians(p2.coordinates.lng - p1.coordinates.lng);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(Radians(p1.coordinates.lat)) * Math.Cos(Radians(p2.coordinates.lat)) *
              Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            //Logger.Trace(String.Format("Distance between locations {0}[{1},{2}] and {3}[{4},{5}] is {6}", p1.address, p1.coordinates.lat, p1.coordinates.lng, p2.address, p2.coordinates.lat, p2.coordinates.lng, d));
            return d; // returns the distance in miles
        }
    }
}