using Dapper;
using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    public class Location
    {
        public Coordinates Coordinates { get; set; } = new Coordinates();
        public CartesianCoordinates CartesianCoordinates { get; set; } = new CartesianCoordinates();
        public List<Location> Neighbors { get; set; } = new List<Location>();

        public Vehicle AssignedVehicle { get; set; }
        public double CurrentGallonsEstimate { get; set; }
        public DateTime? intendedPickupDate { get; set; } = default(DateTime);

        public int? TrackingNumber { get; set; }
        public int RouteId { get; set; }
        public int Id { get; set; } = int.MinValue;
        public int? ClientPriority { get; set; } = 1;
        public int? OilPickupSchedule { get; set; }
        public int? VehicleSize { get; set; }
        public string Account { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public double? OilTankSize { get; set; }
        public double? DaysUntilDue { get; set; }
        public double? DistanceFromDepot { get; set; }

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
        public int? GreaseTrapUnits { get; set; }
        public string GreaseTrapPreferredDay { get; set; }
        public int? GreaseTrapSchedule { get; set; }
        public int? NumberOfManHoles { get; set; }
    }

    public class Coordinates
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        //created a default constructor for dapper to pass in null class objects and create a default object
        public Coordinates() {}
        public Coordinates(Coordinates c) {}
    }

    public class CartesianCoordinates
    {
        public double X { get; set; } = Double.NaN;
        public double Y { get; set; } = Double.NaN;
        public double Z { get; set; } = Double.NaN;
        //created a default constructor for dapper to pass in null class objects and create a default object
        public CartesianCoordinates() { }
        public CartesianCoordinates(CartesianCoordinates c) { }
    }

    public class CoordinatesMap : EntityMap<Coordinates>
    {
        public CoordinatesMap()
        {
            Map(c => c.Lat).ToColumn("coordinates_latitude");
            Map(c => c.Lng).ToColumn("coordinates_longitude");
        }
    }

    public class SphericalCoordinatesMap : EntityMap<CartesianCoordinates>
    {
        public SphericalCoordinatesMap()
        {
            Map(c => c.X).ToColumn("cartesian_x");
            Map(c => c.Y).ToColumn("cartesian_y");
            Map(c => c.Z).ToColumn("cartesian_z");
        }
    }
}