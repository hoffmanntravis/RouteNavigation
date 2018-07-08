using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    public class Location
    { 
    public double lat = Double.NaN;
    public double lng = Double.NaN;

    public int clientPriority;
        public int id;
        public int pickupIntervalDays;
        public int vehicleSize = int.MaxValue;
        public Vehicle assignedVehicle;
        public string locationName;
        public string address;
        public string contactName;
        public string contactEmail;
        public double capacityGallons = 0;
        public double daysUntilDue = int.MinValue;
        public double matrixWeight;
        public double distanceFromSource;

        public DateTime lastVisited = default(DateTime);
        public double currentGallonsEstimate;
        public TimeSpan visitTime;
        public List<Location> neighbors = new List<Location>();
    }
}