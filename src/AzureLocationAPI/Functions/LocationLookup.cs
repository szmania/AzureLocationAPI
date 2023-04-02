using AzureLocationAPI.Helpers;
using AzureLocationAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureLocationAPI.Functions
{
    public static class AzureLocationAPI
    {
        [FunctionName("AzureLocationAPI")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "near/{latitude}/{longitude}/{distance:int=16000}")] HttpRequest req,
            [CosmosDB(Constants.CosmosDbName,
                      Constants.MyLocationsCollection,
                      CreateIfNotExists = true,
                      ConnectionStringSetting = "AzureCosmosDBConnectionString",
            SqlQuery ="SELECT * FROM locations l WHERE ST_DISTANCE(l.location, {{ 'type': 'Point', 'coordinates':[ {latitude},{longitude}]}}) < {distance}"
            )] IEnumerable<dynamic> destinations,
            double latitude,
            double longitude,
            ILogger log)
        {
            log.LogInformation("Location Lookup Started");

            List<dynamic> resultList = new List<dynamic>();

            try
            {
                int returnCount;

                string countParam = req.Query["count"];
                if (!string.IsNullOrWhiteSpace(countParam) && countParam.CompareTo("all") == 0)
                {
                    returnCount = destinations.Count();
                }
                else
                {
                    int.TryParse(countParam, out returnCount);

                    if (returnCount < 1)
                    {
                        returnCount = int.Parse(Environment.GetEnvironmentVariable("DefaultReturnCount"));
                    }
                }

                WayPoint userLocation = new WayPoint
                {
                    Longitude = longitude,
                    Latitude = latitude
                };

                BingHelper bh = new BingHelper();

                foreach (dynamic d in destinations.Take(returnCount))
                {
                    var location = d["location"]["coordinates"];
                    if (location != null)
                    {
                        WayPoint wp = new WayPoint
                        {
                            Latitude = location[0],
                            Longitude = location[1]
                        };

                        d.MapUri = await bh.GetMapImageUrl(userLocation, wp);
                        d.Distance = await bh.GetRouteDistance(userLocation, wp);

                        resultList.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            log.LogInformation("Location Lookup Complete");
            return new OkObjectResult(resultList);
        }
    }
}
