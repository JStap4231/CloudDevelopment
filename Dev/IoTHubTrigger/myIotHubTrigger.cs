using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;

using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Company.Function
{

    public class TemperatureItem
  {
    [JsonProperty("id")]
    public string Id {get; set;}
    public double LivingRoomTemp {get; set;}
    public double Humidity {get; set;}
    public bool FrontDoor {get; set;}
    public bool HeatingOn {get; set;}
  }

    public class myIotHubTrigger
    {
        private static HttpClient client = new HttpClient();
        
        [FunctionName("myIoTHubTrigger")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
        [CosmosDB(databaseName: "IoTData",
                                 collectionName: "Temps",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out TemperatureItem output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            var jsonBody = Encoding.UTF8.GetString(message.Body);
            dynamic data = JsonConvert.DeserializeObject(jsonBody);
            double livingroonTemp = data.livingroonTemp;
            double livingroomMoisture = data.livingroomMoisture;
            bool frontDoor = data.frontDoor;
            bool isHeatingOn = data.isHeatingOn;

            output = new TemperatureItem
            {
                LivingRoomTemp = livingroonTemp,
                Humidity = livingroomMoisture,
                HeatingOn = isHeatingOn,
                FrontDoor = frontDoor
            };
        }

        [FunctionName("GetTemperature")]
        public static IActionResult GetTemperature(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "temperature/")] HttpRequest req,
            [CosmosDB(databaseName: "IoTData",
                collectionName: "Temps",
                ConnectionStringSetting = "cosmosDBConnectionString",
                    SqlQuery = "SELECT * FROM c")] IEnumerable< TemperatureItem> temperatureItem,
                ILogger log)
        {
            return new OkObjectResult(temperatureItem);
        }
    }
}