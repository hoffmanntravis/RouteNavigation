using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Threading;
using NLog;

namespace Apis
{
    public class ApiLocation
    {
        private  Logger Logger = LogManager.GetCurrentClassLogger();
        public double latitude;
        public double longitude;
        private string url = System.Configuration.ConfigurationManager.AppSettings["googleGeocodeApiUrl"];
        private string apiKey = System.Configuration.ConfigurationManager.AppSettings["googleApiKey"];
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;
        private string illegalCharactersString = System.Configuration.ConfigurationManager.AppSettings["googleApiIllegalCharacters"];

        public ApiLocation(string address)
        {
            address = address.Replace(" ", "+");
            Api googleApi = new Api();

            url = url + "?address=" + address + "&key=" + apiKey;
            Task.Run(() => googleApi.CallApi(url)).Wait();
            string jsonResponse = googleApi.response;
            RootObject root = JsonConvert.DeserializeObject<RootObject>(jsonResponse);

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

            if (root.Results.Count > 0)
            {
                latitude = root.Results[0].Geometry.Location.Lat;
                longitude = root.Results[0].Geometry.Location.Lng;
            }
            else
            {
                Logger.Error("Unable to parse json coordinates from " + url);
            }
        }

        public class AddressComponent
        {
            public string Long_name { get; set; }
            public string Short_name { get; set; }
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

        public class Location
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Northeast2
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Southwest2
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Viewport
        {
            public Northeast2 Northeast { get; set; }
            public Southwest2 Southwest { get; set; }
        }

        public class Geometry
        {
            public Bounds Bounds { get; set; }
            public Location Location { get; set; }
            public string Location_type { get; set; }
            public Viewport Viewport { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> Address_components { get; set; }
            public string Formatted_address { get; set; }
            public Geometry Geometry { get; set; }
            public string Place_id { get; set; }
            public List<string> Types { get; set; }
        }

        public class RootObject
        {
            public List<Result> Results { get; set; }
            public string Status { get; set; }
        }
    }
}


