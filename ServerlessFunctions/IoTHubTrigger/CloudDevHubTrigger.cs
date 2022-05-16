using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using System.Data;
using System.Linq;


namespace cloud.dev
{
    public class TemperatureItem
    {
        [JsonProperty("id")]
        public string Id {get; set;}
        public double AirLevel {get; set;}
        public string AirRating {get; set;}
        public double NoiseLevel {get; set;}
        public string NoiseRating {get; set;}
    }

    public class AllDataReturn{
        public double AirLevel {get; set;}
        public string AirRating {get; set;}
        public double NoiseLevel {get; set;}
        public string NoiseRating {get; set;}

    }

    public class AveragesReturn{
        public double NoiseLevelAverage {get; set;}
        public double AirLevelAverage {get; set;}

    }

    public class CloudDevHubTrigger
    {
        private static HttpClient client = new HttpClient();
        
        [FunctionName("CloudDevHubTrigger")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
        [CosmosDB(databaseName: "CloudDevData",
                                 collectionName: "DeviceData",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out TemperatureItem output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            var jsonBody = Encoding.UTF8.GetString(message.Body);
            dynamic data = JsonConvert.DeserializeObject(jsonBody);
            double currentAir = data.currentAir;
            double currentNoise = data.currentNoise;
            string airRating = data.airRating;
            string noiseRating = data.noiseRating;

            output = new TemperatureItem
            {
                AirLevel = currentAir,
                AirRating = airRating,
                NoiseLevel = currentNoise,
                NoiseRating = noiseRating
            };
        }

        [FunctionName("GetAllTimeData")]
        public static IActionResult GetAllTimeData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AllTime/")] HttpRequest req,
        [CosmosDB(databaseName: "CloudDevData",
                  collectionName: "DeviceData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT c.AirLevel, c.AirRating, c.NoiseLevel, c.NoiseRating FROM c")] IEnumerable<AllDataReturn> AllDataReturn,
                  ILogger log)
        {
            return new OkObjectResult(AllDataReturn);
        }

        [FunctionName("GetHourlyData")]
        public static IActionResult GetHourlyData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Hourly/")] HttpRequest req,
        [CosmosDB(databaseName: "CloudDevData",
                  collectionName: "DeviceData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT c.AirLevel, c.AirRating, c.NoiseLevel, c.NoiseRating FROM c WHERE (c._ts * 1000) > (GetCurrentTimeStamp() - 3600000)")] IEnumerable<AllDataReturn> AllDataReturn,
                  ILogger log)
        {
             return new OkObjectResult(AllDataReturn);
        }

        [FunctionName("GetDailyData")]
        public static IActionResult GetDailyData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Daily/")] HttpRequest req,
        [CosmosDB(databaseName: "CloudDevData",
                  collectionName: "DeviceData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT c.AirLevel, c.AirRating, c.NoiseLevel, c.NoiseRating FROM c WHERE (c._ts * 1000) > (GetCurrentTimeStamp() - 86400000)")] IEnumerable<AllDataReturn> AllDataReturn,
                  ILogger log)
        {
            return new OkObjectResult(AllDataReturn);
        }
        [FunctionName("GetDailyAverage")]
        public static IActionResult GetDailyAverage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DailyAverages/")] HttpRequest req,
        [CosmosDB(databaseName: "CloudDevData",
                  collectionName: "DeviceData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT AVG(c.NoiseLevel) AS \"NoiseLevelAverage\", AVG(c.AirLevel) AS \"AirLevelAverage\" FROM c WHERE (c._ts * 1000) > (GetCurrentTimeStamp() - 86400000)")] IEnumerable<AveragesReturn> AveragesReturn,
                  ILogger log)
        {
            return new OkObjectResult(AveragesReturn);
        }
        
        [FunctionName("GetHourlyAverages")]
        public static IActionResult GetHourlyAverages(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "HourlyAverages/")] HttpRequest req,
        [CosmosDB(databaseName: "CloudDevData",
                  collectionName: "DeviceData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT AVG(c.NoiseLevel) AS \"NoiseLevelAverage\", AVG(c.AirLevel) AS \"AirLevelAverage\" FROM c WHERE (c._ts * 1000) > (GetCurrentTimeStamp() - 3600000)")] IEnumerable<AveragesReturn> AveragesReturn,
                  ILogger log)
        {
            return new OkObjectResult(AveragesReturn);
        }

        [FunctionName("Verifier")]
        public static IActionResult Verifier(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "loaderio-8802f0eb2ffe4725055ac57a97198089")] HttpRequest req,
        ILogger log) {
            return new OkObjectResult("loaderio-8802f0eb2ffe4725055ac57a97198089");
        }

    }
}