using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    public class Route
    {
        public int Id;
        public TimeSpan TotalTime;
        public DateTime Date;
        public double DistanceMiles;
        public Location Origin;
        public Color Color = new Color();
        public List<Location> Waypoints = new List<Location>();
        public string MapsUrl;
        public Vehicle AssignedVehicle;
        public List<Location> AllLocations = new List<Location>();
        public double AverageLocationDistance;
    }
}