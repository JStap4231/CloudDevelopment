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

namespace cloud.dev
{
    public class TemperatureItem
    {
        [JsonProperty("id")]
        public string Id {get; set;}
        public double LivingroomTemp {get; set;}
        public double BedroomTemp {get; set;}
        public bool DoorStatus {get; set;}
        public bool HeatingStauts {get; set;}
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
            double currentLivingroomTemperature = data.currentLivingroomTemperature;
            double currentBedroomTemperature = data.currentBedroomTemperature;
            bool isHeatingOn = data.isHeatingOn;
            bool doorLocked = data.doorLocked;

            output = new TemperatureItem
            {
                LivingroomTemp = currentLivingroomTemperature,
                BedroomTemp = currentBedroomTemperature,
                DoorStatus = doorLocked,
                HeatingStauts = isHeatingOn
            };
        }
        [FunctionName("GetTemperature")]
        public static IActionResult GetTemperature(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "temperature/")] HttpRequest req,
        [CosmosDB(databaseName: "CloudDevData",
                  collectionName: "DeviceData",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT * FROM c")] IEnumerable< TemperatureItem> temperatureItem,
                  ILogger log)
        {
            return new OkObjectResult(temperatureItem);
        }
    }
}