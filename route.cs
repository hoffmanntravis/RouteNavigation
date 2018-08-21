using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    public class Route
    {
        public int id;
        public TimeSpan totalTime;
        public DateTime date;
        public double distanceMiles;
        public Location origin;

        public List<Location> waypoints = new List<Location>();
        public string mapsUrl;
        public Vehicle assignedVehicle;
        public List<Location> allLocations = new List<Location>();
        public double averageLocationDistance;

        public Route(Location o)
        {
            origin = o;
        }

    }
}