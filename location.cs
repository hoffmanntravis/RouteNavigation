using Dapper;
using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace RouteNavigation
{
    [Serializable]
    public class Location
    {
        
        public Coordinates Coordinates { get; set; } = new Coordinates();
        public CartesianCoordinates CartesianCoordinates { get; set; } = new CartesianCoordinates();

        public Vehicle AssignedVehicle { get; set; }
        public double CurrentGallonsEstimate { get; set; }
        public DateTime? IntendedPickupDate { get; set; }
        public List<Location> Neighbors { get; set; } = new List<Location>();

        public int? TrackingNumber { get; set; }
        public int RouteId { get; set; }
        public int Id { get; set; } = int.MinValue;
        public int? ClientPriority { get; set; } = 1;
        public int? OilPickupSchedule { get; set; } = 30;
        public int? VehicleSize { get; set; }
        public string Account { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public double? OilTankSize { get; set; }
        public double? OilPickupDaysUntilDue { get; set; }
        public double DistanceFromSource { get; set; }
        public double? DaysUntilDue { get; set; }
        public DateTime? OilPickupLastScheduledService { get; set; }
        public DateTime? GreaseTrapLastScheduledService { get; set; }
        public double? OilDaysElapsed { get; set; }
        public double? GreaseDaysElapsed { get; set; }
        public double? DaysElapsed { get; set; }

        public bool OilPickupCustomer { get; set; } = false;
        public bool? OilPickupSignatureRequired { get; set; }
        public DateTime? OilPickupNextDate { get; set; }
        public string OilPickupServiceNotes { get; set; }

        public TimeSpan? GreaseTrapPreferredTimeStart { get; set; } = Config.Calculation.workdayStartTime;
        public TimeSpan? GreaseTrapPreferredTimeEnd { get; set; } = Config.Calculation.workdayEndTime;
        public double? GreaseTrapSize { get; set; }
        public DateTime? GreaseTrapPickupNextDate { get; set; }
        public bool GreaseTrapCustomer { get; set; } = false;
        public string GreaseTrapServiceNotes { get; set; }
        public bool? GreaseTrapSignatureRequired { get; set; }
        public double? GreaseTrapDaysUntilDue { get; set; }
        public int? GreaseTrapUnits { get; set; }
        public string GreaseTrapPreferredDay { get; set; }
        public int? GreaseTrapSchedule { get; set; } = 30;
        public int? NumberOfManHoles { get; set; }
        public Location nearestLocation { get; set; }
        public double? distanceToNearestLocation { get; set; } = null;

        public Location DeepClone(Location l)
        {
            l.Neighbors.Clear();
            l.nearestLocation = null;
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, l);
                ms.Position = 0;

                return (Location)formatter.Deserialize(ms);
            }
        }
    }

    public class Neighbors
    {
        private Dictionary<Location, List<Location>> neighborsDictionary = new Dictionary<Location, List<Location>>();
        public void AddNeighbors (Location l, List<Location> neighbors)
        {
            neighborsDictionary.Add(l,neighbors);
        }

        public List<Location> GetNeighbors(Location l)
        {
            return neighborsDictionary[l];
        }
    }

    [Serializable]
    public class Coordinates
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        //created a default constructor for dapper to pass in null class objects and create a default object
        public Coordinates() { }
        public Coordinates(Coordinates c)
        {
            if (c != null)
            {
                Lat = c.Lat;
                Lng = c.Lng;
            }
        }
    }
    [Serializable]
    public class CartesianCoordinates
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
        //created a default constructor for dapper to pass in null class objects and create a default object
        public CartesianCoordinates() { }
        public CartesianCoordinates(CartesianCoordinates c)
        {
            if (c != null)
            {
                X = c.X;
                Y = c.Y;
                Z = c.Z;
            }
        }
    }
    [Serializable]
    public class CoordinatesMap : EntityMap<Coordinates>
    {
        public CoordinatesMap()
        {
            Map(c => c.Lat).ToColumn("coordinates_latitude");
            Map(c => c.Lng).ToColumn("coordinates_longitude");
        }
    }
    [Serializable]
    public class CartesianCoordinatesMap : EntityMap<CartesianCoordinates>
    {
        public CartesianCoordinatesMap()
        {
            Map(c => c.X).ToColumn("cartesian_x");
            Map(c => c.Y).ToColumn("cartesian_y");
            Map(c => c.Z).ToColumn("cartesian_z");
        }
    }



}