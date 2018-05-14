using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RouteNavigation
{
    public class Config
    {
        public Matrix matrix = new Matrix();
        public Calculation Calculation = new Calculation();
        public Features Features = new Features();
        public int maximumDaysOverdue = 60;
        static string logLevelString = "INFO";
        public static string logLevel = logLevelString.ToUpper();
    }

    public class Matrix
    {
        public double priorityMultiplier = 1;
        public double daysUntilDueExponent = 1;
        public double distanceFromSourceMultiplier = 1;
        public double overDueMultiplier = 1;
    }

    public class Calculation
    {
        protected DataAccess dataAccess = new DataAccess();
        public static Location origin;

        private int _originLocationId;
        public int OriginLocationId
        {
            get { return _originLocationId; }
            set
            {
                if (OriginLocationId != value)
                {
                    _originLocationId = value;
                    SetOriginLocation(_originLocationId);
                }

            }
        }

        public double currentFillLevelErrorMarginPercent = 0;
        public double routeMaxHours = 12;
        public double oilPickupAverageDurationMinutes = 30;
        public double greasePickupAverageDurationMinutes = 30;
        public int minimDaysUntilPickup = 0;

        protected void SetOriginLocation(int id)
        {
            //only set origin once since it is a static variable, and we don't want to call the DB every time origin is requested
            if (origin == null)
            {
                Location o = dataAccess.GetLocationById(id);
                origin = o;
            }
        }
    }

    public class Features
    {
        public bool vehicleFillLevel = false;
        public bool prioritizeNearestLocation = false;
    }


}