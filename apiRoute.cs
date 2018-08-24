using Newtonsoft.Json;
using NLog;
using Npgsql;
using RouteNavigation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RouteNavigation
{

    public class ApiRoute
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public string mapsUrl;
        public List<Location> waypoints = new List<Location>();
        public TimeSpan duration;
        public Route googleProperties;

        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        protected string directionsApiUrl = System.Configuration.ConfigurationManager.AppSettings["googleDirectionsApiUrl"];
        protected string apiKey = System.Configuration.ConfigurationManager.AppSettings["googleApiKey"];
        protected string mapsBaseUrl = System.Configuration.ConfigurationManager.AppSettings["googleDirectionsMapsUrl"];
        protected string illegalCharactersString = System.Configuration.ConfigurationManager.AppSettings["googleApiIllegalCharacters"];

        public ApiRoute(RouteNavigation.Route route)
        {
            //API's return an object into a class, where we create a 'RootObject' that holds all subproperties of the rest data.
            GoogleDirections directions = GetRootApiObject(route);
            SetGoogleApiObjectProperties(directions);
            mapsUrl = BuildMapsUrl(route);
        }

        protected void SetGoogleApiObjectProperties(GoogleDirections directions)
        {
            if (directions.Routes.Count > 0)
            {
                Route googleRoute = directions.Routes.FirstOrDefault();
                googleProperties = directions.Routes.FirstOrDefault();
                foreach (Leg leg in googleRoute.Legs)
                {
                    foreach (Step step in leg.Steps)
                    {
                        Location stepLocation = new Location();
                        stepLocation.coordinates.lat = step.End_location.Lat;
                        stepLocation.coordinates.lng = step.End_location.Lng;
                        waypoints.Add(stepLocation);
                    }
                }
            }
            else
            {
                Logger.Error("ThreadId:" + Thread.CurrentThread.ManagedThreadId.ToString() + " "+ "Unable to parse json properties from " + directionsApiUrl);
            }
        }

        public string BuildMapsUrl(RouteNavigation.Route route)
        {
            mapsUrl = mapsBaseUrl;
            if (route.allLocations.Count > 0)
                foreach (Location waypoint in route.allLocations)
                    mapsUrl += waypoint.address + "/";

            //make the url google friendly.  Remove illegal characters and replace them with spaces.
            mapsUrl = mapsUrl.Replace(" ", "+");
            mapsUrl = mapsUrl.Replace("#", " ");
            mapsUrl = ReplaceIllegalCharaters(mapsUrl);
            LogEventInfo logEvent = new LogEventInfo(NLog.LogLevel.Trace,Logger.Name,"Constructed google maps url: " + mapsUrl);

            return mapsUrl;
        }

        protected GoogleDirections GetRootApiObject(RouteNavigation.Route route)
        {
            Api googleApi = new Api();

            directionsApiUrl = directionsApiUrl + "?optimize:true";
            directionsApiUrl = directionsApiUrl + "&origin=" + route.origin.address;

            if (waypoints.Count > 0)
            {
                directionsApiUrl = directionsApiUrl + "&waypoints=";
                foreach (Location waypoint in route.waypoints)
                    directionsApiUrl = directionsApiUrl + waypoint.address + "|";
            }
            directionsApiUrl = directionsApiUrl + "&destination=" + route.origin.address;
            directionsApiUrl = directionsApiUrl + "&key=" + apiKey;
            directionsApiUrl = directionsApiUrl.Replace(" ", "+");
            directionsApiUrl = ReplaceIllegalCharaters(directionsApiUrl);

            Task.Run(() => googleApi.CallApi(directionsApiUrl)).Wait();
            string jsonResponse = googleApi.response;
            GoogleDirections root = JsonConvert.DeserializeObject<GoogleDirections>(jsonResponse);

            if (root.Status == "OVER_QUERY_LIMIT")
            {
                Exception exception = new Exception("Google API returned Status:" + root.Status + ".  This is considered fatal.  Please check your api usage, or check with an administrator as to why this status is occurring.");
                Logger.Error(exception);
                throw exception;

            }
            else if (root.Status != "OK")
            {
                Exception exception = new Exception("Google API returned Status:" + root.Status + ".  Please check your api usage, or check with an administrator as to why this status is occurring.");
                Logger.Error(exception);
            }

            return root;
        }
        /*
        public Route sortRouteTSP(RouteNavigation.Route route)
        {
            int index = 0;
            foreach (Location location in route.waypoints)
            {
                location.index = index;
                index += 1;
            }
            List<GeocodedWaypoint> waypointsSorted = GetRootApiObject(route).Geocoded_waypoints;

        }
        */

        public string ReplaceIllegalCharaters(string s)
        {
            char[] illegalCharacters = illegalCharactersString.ToCharArray();
            foreach (char c in illegalCharacters)
            {
                s = s.Replace(c.ToString(), "");
            }
            return s;
        }

        public class GeocodedWaypoint
        {
            public string Geocoder_status { get; set; }
            public string Place_id { get; set; }
            public List<string> Types { get; set; }
        }

        public class Northeast
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Southwest
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Bounds
        {
            public Northeast Northeast { get; set; }
            public Southwest Southwest { get; set; }
        }

        public class Distance
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class Duration
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class EndLocation
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class StartLocation
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Polyline
        {
            public string Points { get; set; }
        }

        public class StartLocation2
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Step
        {
            public Distance Distance { get; set; }
            public Duration Duration { get; set; }
            public EndLocation End_location { get; set; }
            public string Html_instructions { get; set; }
            public Polyline Polyline { get; set; }
            public StartLocation2 Start_location { get; set; }
            public string Travel_mode { get; set; }
            public string Maneuver { get; set; }
        }

        public class Leg
        {
            public Distance Distance { get; set; }
            public Duration Duration { get; set; }
            public string End_address { get; set; }
            public EndLocation End_location { get; set; }
            public string Start_address { get; set; }
            public StartLocation Start_location { get; set; }
            public List<Step> Steps { get; set; }
            public List<object> Traffic_speed_entry { get; set; }
            public List<object> Via_waypoint { get; set; }
        }

        public class OverviewPolyline
        {
            public string Points { get; set; }
        }

        public class Route
        {
            public Bounds Bounds { get; set; }
            public string Copyrights { get; set; }
            public List<Leg> Legs { get; set; }
            public OverviewPolyline Overview_polyline { get; set; }
            public string Summary { get; set; }
            public List<object> Warnings { get; set; }
            public List<int> Waypoint_order { get; set; }
        }

        public class GoogleDirections
        {
            public List<GeocodedWaypoint> Geocoded_waypoints { get; set; }
            public List<Route> Routes { get; set; }
            public string Status { get; set; }
        }
    }
}