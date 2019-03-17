using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using NLog;
using RouteNavigation;

namespace Apis
{
    public class Api
    {
        private  Logger Logger = LogManager.GetCurrentClassLogger();
        public string response;
        private string illegalCharactersString = System.Configuration.ConfigurationManager.AppSettings["googleApiIllegalCharacters"];
        
        public async Task CallApi(string url)
        {

            url = url.Replace(" ", "+");
            url = url.Replace("#", " ");
            url = ReplaceIllegalCharaters(url);
            try
            {
                Logger.Info("Calling: " + url);
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
                    response = await responseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception exception)
            {
               Logger.Error(exception);
            }
            try
            {
                DataAccess.UpsertApiMetadata();
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to insert record using upsert_api_metadata stored procedure.");
                Logger.Error(exception);
            }
        }

        public string ReplaceIllegalCharaters(string s)
        {
            char[] illegalCharacters = illegalCharactersString.ToCharArray();
            foreach (char c in illegalCharacters)
            {
                s = s.Replace(c.ToString(), "");
            }
            return s;
        }
    }
}