using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace RouteNavigation
{
    [Serializable]
    public class Route
    {
       public List<Location> AllLocations = new List<Location>();
         public List<Location> Waypoints = new List<Location>();
        public int Id;
        public TimeSpan TotalTime;
        public DateTime Date;
        public double DistanceMiles;
        public Location Origin;
        public Color Color = new Color();
        public string MapsUrl;
        public Vehicle AssignedVehicle;
        public double AverageLocationDistance;

        public Route DeepClone(Route l)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, l);
                ms.Position = 0;

                return (Route)formatter.Deserialize(ms);
            }
        }
    }

}