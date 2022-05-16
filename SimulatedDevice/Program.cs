// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    /// <summary>
    /// This sample illustrates the very basics of a device app sending telemetry. For a more comprehensive device app sample, please see
    /// <see href="https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/master/iot-hub/Samples/device/DeviceReconnectionSample"/>.
    /// </summary>
    internal class Program
    {
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private static string s_connectionString = "HostName=CloudDevIoTHub.azure-devices.net;DeviceId=JStap_SimDevice;SharedAccessKey=TKy8DAdR0vH16wvIw/lzKj1r0hlpVuTEFnQz0Fa68og=";
		

        private static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts #1 - Simulated device.");

            // This sample accepts the device connection string as a parameter, if present
            ValidateConnectionString(args);

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync(cts.Token);

            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        private static void ValidateConnectionString(string[] args)
        {
            if (args.Any())
            {
                try
                {
                    var cs = IotHubConnectionStringBuilder.Create(args[0]);
                    s_connectionString = cs.ToString();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error: Unrecognizable parameter '{args[0]}' as connection string.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    _ = IotHubConnectionStringBuilder.Create(s_connectionString);
                }
                catch (Exception)
                {
                    Console.WriteLine("This sample needs a device connection string to run. Program.cs can be edited to specify it, or it can be included on the command-line as the only parameter.");
                    Environment.Exit(1);
                }
            }
        }

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
            // Initial telemetry values
            double air = 50;
            double noise =  50;
            string airRating;
            string noiseRating;
            var rand = new Random();

            while (!ct.IsCancellationRequested)
            {
                //Calculate the new air pollution reading
                double currentAir = air + rand.NextDouble() * 150;

                //Calculate the new noise pollution reading
                double currentNoise = noise + rand.NextDouble() * 100;
                
                //Depending on the value of the air pollution, give it an appropriate rating.
                if (currentAir <= 60){
                    airRating = "low";
                }else if (currentAir > 60 & currentAir <= 110){
                    airRating = "elevated";
                }else if(currentAir > 110 & currentAir <= 160){
                    airRating = "high";
                }else{
                    airRating = "very high";
                }

                //Depending on the noise polluttion level, give it an appropriate rating.
                if (currentNoise <= 65){
                    noiseRating = "low";
                }else if (currentNoise > 65 & currentNoise <= 75){
                    noiseRating = "elevated";
                }else{
                    noiseRating = "high";
                }

                // Create JSON message
                string messageBody = JsonSerializer.Serialize(
                    new
                    {
                        currentAir,
                        airRating,
                        currentNoise,
                        noiseRating
                    });
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                await Task.Delay(3000);
            }
        }
    }
}
