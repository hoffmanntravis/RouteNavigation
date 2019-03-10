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
        [ThreadStatic] public static List<Route> _routes;
        [ThreadStatic] public static Metadata _metadata;
        [ThreadStatic] static List<Location> _availableLocations;
        [ThreadStatic] static List<Vehicle> _availableVehicles;
        [ThreadStatic] static List<Vehicle> _currentVehicles;
        [ThreadStatic] static List<Location> _orphanedLocations;
        [ThreadStatic] static List<Location> _serviceNowLocations;
        [ThreadStatic] static List<Location> _compatibleLocations;
        [ThreadStatic] static Route _potentialRoute;
        [ThreadStatic] static DateTime _currentTime;
        [ThreadStatic] static DateTime _potentialTime;
        [ThreadStatic] static Location _previousLocation;
        [ThreadStatic] static Location _nextLocation;
        [ThreadStatic] static Vehicle _vehicle;
        [ThreadStatic] static Location _nearestLocation;
        [ThreadStatic] static List<Location> _availableLocationsWithPostponedLocations;
        [ThreadStatic] static List<Location> _postPonedLocations;
        [ThreadStatic] static Location _firstDueLocation;
        [ThreadStatic] static double _currentDistance;
        [ThreadStatic] static double _potentialDistance;
        [ThreadStatic] static DateTime _startTime;
        [ThreadStatic] static DateTime _endTime;
        [ThreadStatic] static DateTime _startDate;
        [ThreadStatic] static double _nextLocationDistanceMiles;
        [ThreadStatic] static double _distanceTolerance;
        [ThreadStatic] static double _distanceToDepotFromLastWaypoint;
        [ThreadStatic] static TimeSpan _travelTimeBackToDepot;
        [ThreadStatic] static TimeSpan _travelTime;

        public Guid activityId;

        public uint neighborCount = 60;
        private static object addLock = new object();

        public RouteCalculator(List<Location> locations, List<Vehicle> vehicles)
        {
            CalculateRoutes(locations, vehicles);
        }

        private void CalculateRoutes(List<Location> locations, List<Vehicle> vehicles)
        {
            activityId = Trace.CorrelationManager.ActivityId;
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();

            _startDate = AdvanceDateToNextWeekday(System.DateTime.Now.Date);
            _orphanedLocations = new List<Location>();
            _metadata = new Metadata();
            _routes = new List<Route>();

            lock (addLock)
                _availableLocations = new List<Location>(locations);
            lock (addLock)
                _availableVehicles = new List<Vehicle>(vehicles);
            lock (addLock)
                _currentVehicles = new List<Vehicle>(vehicles);


            _metadata.intakeLocations = _availableLocations.ToList();
            _startTime = _startDate + Config.Calculation.workdayStartTime;
            _endTime = _startDate + Config.Calculation.workdayEndTime;

            //availableLocations = NearestNeighbor(availableLocations);

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

                while (_availableLocations.Count > 0)
                {
                    if (_currentVehicles.Count == 0)
                    {
                        //get some more vehicles and start a new day, with new routes
                        _currentVehicles = _availableVehicles.ToList();
                        _startDate = AdvanceDateToNextWeekday(_startDate);
                    }
                    Logger.Trace("startdate is {0}", _startDate);
                    _startTime = _startDate + Config.Calculation.workdayStartTime;
                    _endTime = _startDate + Config.Calculation.workdayEndTime;


                    //Remove any locations that would be picked up too soon to be relevent.  We'll invoke a recursive call at the end to deal with these.

                    _availableLocationsWithPostponedLocations = _availableLocations.ToList();
                    _postPonedLocations = GetLaterDateLocations(_availableLocations, _startTime).ToList();

                    _availableLocations = _availableLocations.Except(_postPonedLocations).ToList();

                    //If all that is left are locations that need to be processed later, advance the date accordingly
                    if (_postPonedLocations.Count > 0 && _availableLocations.Count == 0)
                    {

                        _firstDueLocation = _postPonedLocations.OrderBy(a => a.DaysUntilDue).First();
                        //double daysElapsed = _firstDueLocation.DaysElapsed.Value;
                        //double daysToAdd = Config.Calculation.MinimumDaysUntilPickup - daysElapsed;
                        _currentVehicles = _availableVehicles.ToList();
                        _startDate = _startDate.AddDays(1);

                        _availableLocations = _postPonedLocations.ToList();
                        if (_startDate.DayOfWeek == DayOfWeek.Saturday || _startDate.DayOfWeek == DayOfWeek.Sunday)
                            _startDate = AdvanceDateToNextWeekday(_startDate);

                        continue;
                    }

                    _serviceNowLocations = GetRequireServiceNowLocations(_availableLocations, _startTime);
                    if (_serviceNowLocations.Count > 0)
                    {
                        _availableLocations = _availableLocations.Except(_serviceNowLocations).ToList();
                        _availableLocations.InsertRange(0, _serviceNowLocations);
                    }

                    //sort vehicles by size descending.  We do this to ensure that large vehicles are handled first since they have a limited location list available to them.

                    _currentVehicles.Sort((a, b) => b.physicalSize.CompareTo(a.physicalSize));

                    _vehicle = _currentVehicles.First();
                    _compatibleLocations = GetCompatibleLocations(_vehicle, _availableLocations);
                    _potentialRoute = new Route();
                    _currentTime = _startTime;

                    _currentDistance = 0;
                    _potentialRoute.AllLocations.Add(Config.Calculation.origin);

                    _previousLocation = Config.Calculation.origin;
                    while (_compatibleLocations.Count > 0)
                    {
                        _potentialTime = _currentTime;
                        _potentialDistance = _currentDistance;

                        _nextLocation = _compatibleLocations.First();

                        if (_compatibleLocations.Count > 1)
                        {

                            _nearestLocation = FindNearestLocation(_previousLocation, _compatibleLocations);
                            if (CalculateDistance(_previousLocation, _nearestLocation) == 0)
                                _nextLocation = _nearestLocation;
                        }

                        if (Config.Features.vehicleFillLevel == true)
                        {
                            _nextLocation.CurrentGallonsEstimate = EstimateLocationGallons(_nextLocation);
                            if (!(CheckVehicleCanAcceptMoreLiquid(_vehicle, _nextLocation)))
                            {
                                Logger.Trace(String.Format("Performing a dropoff.  This will take {0} minutes.  Resetting current gallons to 0.", Config.Calculation.dropOffTime));

                                _vehicle.currentGallons = 0;
                                _potentialTime.Add(Config.Calculation.dropOffTime);
                            }
                        }

                        _nextLocationDistanceMiles = CalculateDistance(_previousLocation, _nextLocation);
                        _distanceTolerance = (double)_nextLocation.DistanceFromDepot * (Config.Calculation.searchRadiusFraction);
                        if (_potentialRoute.Waypoints.Count > 0)
                        {
                            if (_nextLocationDistanceMiles >= Math.Max(_distanceTolerance, Config.Calculation.searchMinimumDistance))
                            {
                                Logger.Trace(String.Format("Removing location {1} from compatible locations. Distance from {1} to {0} is greater than the radius search tolerance of {2} miles.", _nextLocation.Account, _previousLocation.Account, _distanceTolerance));
                                _compatibleLocations.Remove(_nextLocation);
                                continue;
                            }
                            else
                                Logger.Trace(String.Format("Distance from {1} to {0} is less than the radius search tolerance of {2} miles.  Will not remove from compatible locations.", _nextLocation.Account, _previousLocation.Account, _distanceTolerance));
                        }
                        else
                            Logger.Trace(String.Format("Route currently has 0 locations.  Adding {0} to populate the route.", _nextLocation));

                        _travelTime = CalculateTravelTime(_nextLocationDistanceMiles);
                        Logger.Trace(String.Format("Travel time from {0} ({1}) to next location {2} ({3}) is {4} minutes", _previousLocation.Account, _previousLocation.Address, _nextLocation.Account, _nextLocation.Address, _travelTime.TotalMinutes));

                        _potentialTime += _travelTime;

                        if (_nextLocation.OilPickupCustomer == true)
                            _potentialTime += TimeSpan.FromMinutes(Config.Calculation.oilPickupAverageDurationMinutes);

                        if (_nextLocation.GreaseTrapCustomer == true)
                            _potentialTime += TimeSpan.FromMinutes(Config.Calculation.greasePickupAverageDurationMinutes);

                        //get the current total distance, including the trip back to the depot for comparison to max distance setting

                        _potentialRoute.DistanceMiles += _nextLocationDistanceMiles;
                        //Logger.Trace(string.Format("potential route distance is {0} compared to a threshold of {1}", potentialRoute.distanceMiles, config.Calculation.routeDistanceMaxMiles));

                        if (_potentialRoute.DistanceMiles is double.NaN)
                        {
                            Logger.Error(String.Format("Locations are {0} and {1} with gps coordinates of {2}:{3} and {4}:{5}", Config.Calculation.origin.Account, _nextLocation.Account, Config.Calculation.origin.Coordinates.Lat, Config.Calculation.origin.Coordinates.Lng, _nextLocation.Coordinates.Lat, _nextLocation.Coordinates.Lng));
                            Logger.Error("potentialRoute.distanceMiles is null");
                        }

                        //double localRadiusTolerance = nextLocation.distanceFromDepot / localRadiusDivisor;
                        //This is only relevent if we have a waypoint in the route.  Otherwise, we may end up with no valid locations.  
                        if (_potentialRoute.Waypoints.Count > 0)
                        {
                            //if the location is within a certain radius, even if it means the day length being exceeded
                            if (_potentialTime > _endTime)
                            {
                                Logger.Trace(String.Format("Removing location {0}.  Adding this location would put the route time at {1} which is later than {2}", _nextLocation.Account, _potentialTime, _endTime));

                                _compatibleLocations.Remove(_nextLocation);
                                continue;
                            }
                        }

                        //Made it past any checks that would preclude this nearest route from getting added, add it as a waypoint on the route
                        if (Config.Features.vehicleFillLevel == true)
                            _vehicle.currentGallons += _nextLocation.CurrentGallonsEstimate;

                        _nextLocation.IntendedPickupDate = _potentialTime;
                        _potentialRoute.Waypoints.Add(_nextLocation);
                        _availableLocations.Remove(_nextLocation);
                        _compatibleLocations.Remove(_nextLocation);
                        _currentTime = _potentialTime;
                        _previousLocation = _nextLocation;
                    }

                    //Add the time to travel back to the depot

                    _distanceToDepotFromLastWaypoint = CalculateDistance(_previousLocation, Config.Calculation.origin);
                    _travelTimeBackToDepot = CalculateTravelTime(_distanceToDepotFromLastWaypoint);
                    Logger.Trace(String.Format("Travel time back from {0} ({1}) to {2} ({3}) is {4} minutes", _previousLocation.Account, _previousLocation.Address, Config.Calculation.origin.Account, Config.Calculation.origin.Address, _travelTimeBackToDepot.TotalMinutes));
                    _currentTime.Add(_travelTimeBackToDepot);

                    _potentialRoute.AllLocations.AddRange(_potentialRoute.Waypoints);
                    _potentialRoute.AllLocations.Add(Config.Calculation.origin);


                    if (_potentialRoute.Waypoints.Count == 0)
                        throw new Exception("Route waypoints count is 0.  Something went wrong.  This is probably caused by invalid condidtions removing all compatible locations.  This may be a function of the data set or possibly a programming bug.  Discuss with Developer if possible.");

                    _potentialRoute.AssignedVehicle = _vehicle;
                    _potentialRoute.Waypoints.ForEach(r => r.AssignedVehicle = _vehicle);
                    _currentVehicles.Remove(_vehicle);
                    _potentialRoute.Date = _startDate;

                    //potentialRoute = CalculateTSPRouteTwoOpt(potentialRoute);

                    _potentialRoute.TotalTime = _currentTime - _startTime;
                    //int oilLocationsCount = potentialRoute.allLocations.Where(a => a.type == "oil").ToList().Count;
                    //int greaseLocationsCount = potentialRoute.allLocations.Where(a => a.type == "grease").ToList().Count;
                    //Logger.Log(String.Format("there are {0} oil locations and {1} grease locations.", oilLocationsCount, greaseLocationsCount), "DEBUG");

                    _potentialRoute.DistanceMiles = CalculateTotalDistance(_potentialRoute.AllLocations, true);
                    Logger.Trace("TSP calculated a shortest route 'flight' distance of " + _potentialRoute.DistanceMiles);

                    _potentialRoute.AverageLocationDistance = CalculateAverageLocationDistance(_potentialRoute);
                    _routes.Add(_potentialRoute);

                    _availableLocations = _availableLocationsWithPostponedLocations.Except(_potentialRoute.Waypoints).ToList();
                }


                foreach (Route route in _routes)
                {
                    _metadata.processedLocations.AddRange(route.Waypoints);
                    _metadata.routesDuration += route.TotalTime;
                    _metadata.routesLengthMiles += route.DistanceMiles;

                    if (_metadata.routesLengthMiles is double.NaN)
                        Logger.Error("metadata.routesLengthMiles is null");
                }



                if (_routes.Count > 0)
                {

                    _metadata.averageRouteDistanceMiles = CalculateAverageRouteDistance(_routes);
                    _metadata.averageRouteDistanceStdDev = CalculateRoutesStdDev(_routes);
                    _metadata.fitnessScore = _metadata.CalculateFitnessScore();
                }

                else
                    Logger.Error("Unable to create any routes.");


                _metadata.orphanedLocations = _availableLocations.Where(x => !_metadata.processedLocations.Any(y => y.Address == x.Address)).ToList();
                lock (addLock)
                    metadata = _metadata;
                lock (addLock)
                    routes = _routes;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw e;
            }
        }

        private static DateTime AdvanceDateToNextWeekday(DateTime date)
        {
            lock (addLock)
            {
                date = date.AddDays(1);

                if (date.DayOfWeek == DayOfWeek.Saturday)
                    date = date.AddDays(2);
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    date = date.AddDays(1);
                return date;
            }
        }

        public Location FindFarthestLocation(Location source, List<Location> locations)
        {
            lock (addLock)
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
        }

        public double CalculateTotalDistance(List<Location> locations, bool roundTrip = false)
        {
            lock (addLock)
            {
                double totalDistance = 0;

                for (int x = 0; x < locations.Count - 1; x++)
                    totalDistance += CalculateDistance(locations[x], locations[x + 1]);

                if (roundTrip)
                    totalDistance += CalculateDistance(locations[locations.Count - 1], locations[0]);

                return totalDistance;
            }
        }

        public List<Location> ThreeOptSwap(List<Location> route)
        {
            lock (addLock)
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
        }

        public List<Location> TwoOptSwap(List<Location> route)
        {
            lock (addLock)
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
        }

        public List<Location> RunThreeOptSwap(List<Location> locations, int i, int j, int k)
        {
            lock (addLock)
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
        }

        public List<Location> RunTwoOptSwap(List<Location> locations, int i, int j)
        {
            lock (addLock)
            {
                List<Location> newRoute = new List<Location>();

                for (int x = 0; x <= i - 1; x++)
                    newRoute.Add(locations[x]);


                List<Location> reverseLocations = new List<Location>();

                for (int x = i; x <= j; x++)
                    reverseLocations.Add(locations[x]);


                reverseLocations.Reverse();
                newRoute.AddRange(reverseLocations);

                for (int x = j + 1; x < locations.Count; x++)
                    newRoute.Add(locations[x]);


                return newRoute;
            }
        }

        private int GenerateRouteHash(List<Location> locations)
        {
            lock (addLock)
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
        }

        public Route CalculateTSPRouteNN(Route route)
        {
            lock (addLock)
            {
                try
                {
                    Logger.Trace("Attempting to TSP. Rearranging locations...");
                    route.Waypoints = NearestNeighbor(route.Waypoints);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return route;
            }
        }

        public Route CalculateTSPRouteTwoOpt(Route route)
        {
            lock (addLock)
            {
                try
                {
                    Logger.Info("Attempting to TSP. Rearranging locations...");

                    route.Waypoints = TwoOptSwap(route.Waypoints);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return route;
            }
        }

        public Route CalculateTSPRouteThreeOpt(Route route)
        {
            lock (addLock)
            {
                try
                {
                    Logger.Info("Attempting to TSP. Rearranging locations...");

                    route.Waypoints = ThreeOptSwap(route.Waypoints);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
                return route;
            }
        }

        public static List<Location> NearestNeighbor(List<Location> route, Location firstNode = null)
        {
            lock (addLock)
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
        }

        public double CalculateAverageRouteDistance(List<Route> routes)
        {
            lock (addLock)
            {
                double average = 0;
                double totalDistance = 0;

                foreach (Route route in routes)
                    totalDistance += route.DistanceMiles;

                average = totalDistance / routes.Count;
                return average;
            }
        }

        public double CalculateAverageLocationDistance(Route route)
        {
            lock (addLock)
            {
                //remove one location, because origin and destination are the same
                double average = route.DistanceMiles / (route.AllLocations.Count - 1);
                return average;
            }
        }

        private double CalculateRoutesStdDev(List<Route> routes)
        {
            lock (addLock)
            {
                List<double> values = new List<double>();

                foreach (Route route in routes)

                    values.Add(route.DistanceMiles);

                double avg = values.Average();

                return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
            }
        }

        private bool CheckVehicleCanAcceptMoreLiquid(Vehicle vehicle, Location location)
        {
            lock (addLock)
            {
                //Check if the vehicle can accept more gallons.  Also, multiple the total gallons by a percentage.  Finally, check that the vehicle isn't empty, otherwise we're going to visit regadless.
                if (vehicle.currentGallons + location.CurrentGallonsEstimate > vehicle.oilTankSize * ((100 - Config.Calculation.currentFillLevelErrorMarginPercent) / 100) && vehicle.currentGallons != 0)
                    return false;
                return true;
            }
        }

        private double EstimateLocationGallons(Location location)
        {

            lock (addLock)
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

        }

        private List<Location> GetLaterDateLocations(List<Location> availableLocations, DateTime currentDate)
        {
            lock (addLock)
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
        }

        private List<Location> GetRequireServiceNowLocations(List<Location> availableLocations, DateTime currentDate)
        {
            lock (addLock)
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
            lock (addLock)
            {
                List<Location> compatibleLocations = locations.Where(l => vehicle.physicalSize <= l.VehicleSize || l.VehicleSize is null).ToList();
                return compatibleLocations;
            }

        }

        public static List<Location> GetPossibleLocations(List<Vehicle> vehicles, List<Location> locations)
        {
            lock (addLock)
            {
                List<Location> possibleLocations = new List<Location>();
                double smallestVehicle;
                lock (addLock)
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

                    if (location.DistanceFromDepot >= Config.Calculation.maxDistanceFromDepot)
                    {
                        Logger.Debug(String.Format("{0} is being ignored because it's distance from the depot of {1} is farther than the maximimum config distance of {2} miles", location.Account, location.DistanceFromDepot, Config.Calculation.maxDistanceFromDepot));
                        continue;
                    }
                    lock (addLock)
                        possibleLocations.Add(location);
                }
                return possibleLocations;
            }
        }

        public static Location FindNearestLocation(Location source, List<Location> locations)
        {
            lock (addLock)
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
                    }
                    Logger.Trace(nearestLocation.Address + ": is " + shortestDistance + " miles from " + source.Address);
                }

                return nearestLocation;
            }
        }


        private class NeighborsDistance
        {
            public Location neighbor;
            public double distance;
        }

        public static List<Location> FindNeighbors(Location source, List<Location> locations, uint neighborCount = 50)
        {
            lock (addLock)
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

        }

        public List<Location> GetHighestPrioritylocations(List<Location> locations, int count)
        {
            lock (addLock)
            {
                locations.Sort((a, b) => a.DaysUntilDue.Value.CompareTo(b.DaysUntilDue.Value));
                List<Location> highestPrioritylocations = locations.Take(count).ToList();

                Logger.Trace("Got highest priority locations: " + highestPrioritylocations.ToList().ToString());
                return highestPrioritylocations;
            }

        }


        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        private TimeSpan CalculateTravelTime(double distanceMiles)
        {
            lock (addLock)
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
            lock (addLock)
            {
                if (l1.CartesianCoordinates.X == null || l1.CartesianCoordinates.Y == null || l1.CartesianCoordinates.Z == null)
                    throw new Exception(String.Format("Attempting to calculate on null coordinates of a location with address {0}.  This will result in an error.", l1.Address));

                if (l2.CartesianCoordinates.Y == null || l2.CartesianCoordinates.Y == null || l2.CartesianCoordinates.Z == null)
                    throw new Exception(String.Format("Attempting to calculate on null coordinates of a location with address {0}.  This will result in an error.", l2.Address));
                return Math.Sqrt(Math.Pow(l2.CartesianCoordinates.X.Value - l1.CartesianCoordinates.X.Value, 2) + Math.Pow(l2.CartesianCoordinates.Y.Value - l1.CartesianCoordinates.Y.Value, 2) + Math.Pow(l2.CartesianCoordinates.Z.Value - l1.CartesianCoordinates.Z.Value, 2));
            }
        }
    }
}