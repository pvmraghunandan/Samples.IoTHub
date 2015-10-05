using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Samples.IoTHub.Core;
using Samples.IoTHub.Core.Operations;

namespace Sample.IoTHub.Device
{
    internal class Program
    {
        public static string deviceId;
        public static string deviceKey;
        public static string consumerGroup;
        public static IoTHubContext context;

        private static void Main(string[] args)
        {
            parseCommandLineArguments(args);
            var CancellationTokenSource = new CancellationTokenSource();
            // ReadCommands(CancellationTokenSource.Token);
            InitializeIOTHub();
            ReceiveMessages(CancellationTokenSource.Token);
            while (true)
            {
                SendTelemetry().Wait();
                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            }
        }

        public static void InitializeIOTHub()
        {
            context = new IoTHubContext();
            context.Initialize();
        }

        public static async Task SendTelemetry()
        {
            var sendTelemetry = new SendTelemetryOperation(context);
            var operationParameters = new OperationParameters();
            operationParameters.Arguments.Add("deviceid", deviceId);
            operationParameters.Arguments.Add("deviceKey", deviceKey);
            operationParameters.Arguments.Add("message", "sample");
            await sendTelemetry.ExecuteAsync(operationParameters);
            Console.WriteLine("Successfully sent");
        }

        private static void parseCommandLineArguments(string[] args)
        {
            deviceId = args[0];
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                Console.WriteLine("Device Id is empty");
            }

            deviceKey = args[1];
            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                Console.WriteLine("Device key is empty");
            }

            consumerGroup = args[2];
            if (string.IsNullOrWhiteSpace(consumerGroup))
            {
                consumerGroup = "$Default";
            }
        }

        public async static Task ReceiveMessages(CancellationToken token)
        {
            var deviceClient = context.CreateDeviceClient(deviceId, deviceKey);

            while (!token.IsCancellationRequested)
            {
                var message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(20));
                if (message != null)
                {
                    Console.WriteLine("Received Command : ");
                    Console.WriteLine(Encoding.UTF8.GetString(message.GetBytes()));
                    await deviceClient.CompleteAsync(message);
                    Console.WriteLine("Sending Response");
                    var response = new Message(Encoding.UTF8.GetBytes("Response"));
                    response.Properties.Add("CorrelationId", message.MessageId);
                    response.Properties["messageType"] = "interactive";
                    await deviceClient.SendEventAsync(response);
                }
            }
        }
    }
}
