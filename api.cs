using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RouteNavigation
{
    public class Api
    {
        public string response;
        protected string illegalCharactersString = System.Configuration.ConfigurationManager.AppSettings["googleApiIllegalCharacters"];
        
        public async Task CallApi(string url)
        {

            url = url.Replace(" ", "+");
            url = url.Replace("#", " ");
            url = ReplaceIllegalCharaters(url);
            try
            {
                Logging.Logger.LogMessage("Calling: " + url);
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
                    response = await responseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage(exception.ToString());
            }
            try
            {
                DataAccess.UpsertApiMetadata();
            }
            catch (Exception exception)
            {
                Logging.Logger.LogMessage("Unable to insert record using upsert_api_metadata stored procedure.");
                Logging.Logger.LogMessage(exception.ToString());
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