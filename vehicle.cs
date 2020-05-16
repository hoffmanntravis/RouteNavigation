using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    [Serializable]
    public class Vehicle
    {
        public int id;
        public string model;
        public string name;
        public double oilTankSize;
        public double currentGallons = 0;
        public double physicalSize;
        public bool operational;

    }
}