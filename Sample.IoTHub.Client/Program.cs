using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Common;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Samples.IoTHub.Core;
using Samples.IoTHub.Core.Operations;
using Samples.IoTHub.Core.Operations.Device;

namespace Sample.IoTHub.Client
{
    class Program
    {
        public const string SendCommand = "Send Command";
        public const string ProvisionDevice = "Provision Device";
        private static readonly List<string> OperationsSupported = new List<string>
        {
           SendCommand,
           ProvisionDevice
        };

        public static IoTHubContext context;
        static void Main(string[] args)
        {
            InitializeIOTHub();

            FeedbackLoop();

            var cancellationTokenSource = new CancellationTokenSource();

            ReadTelemetry("T5720022", cancellationTokenSource.Token);
            Console.WriteLine("Welcome to the IoT Hub Emulator, please select an operation");
            for (var index = 0; index < OperationsSupported.Count; index++)
            {
                var operation = OperationsSupported[index];
                Console.WriteLine(string.Format("{0}.{1}", index + 1, operation));
            }

            Console.WriteLine(Environment.NewLine);
            int operationNumber;
            do
            {
                Console.WriteLine("Please type the number for the operation you want to execute (Type -1 to exit):");
                var input = Console.ReadLine();

                int.TryParse(input, out operationNumber);
                if (operationNumber <= 0 || operationNumber > OperationsSupported.Count)
                {
                    Console.WriteLine("You entered an invalid operation. Please run the program again");
                    return;
                }

                var operationSelected = OperationsSupported[operationNumber - 1];

                if (!operationSelected.Any())
                {
                    Console.WriteLine(
                        "The Operation you entered is invalid or not available. Please run the program again and select a different profile.");
                    return;
                }

                ProcessOperation(operationSelected).Wait();

                // SendTelemetry().Wait();
            } while (operationNumber > 0);
            // ProcessOperation(operationSelected).Wait();
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
            operationParameters.Arguments.Add("deviceid", "T5720022");
            operationParameters.Arguments.Add("deviceKey", "PVjPkqj3loB1P0VJzMAkIg==");
            operationParameters.Arguments.Add("message", "sample");
            await sendTelemetry.ExecuteAsync(operationParameters);
            Console.WriteLine("Successfully sent");
        }

        public static async Task ProcessOperation(string operationSelected)
        {
            var operationParameters = new OperationParameters();
            switch (operationSelected)
            {
                case ProvisionDevice:
                    Console.WriteLine("Plese enter device Id");
                    var input = Console.ReadLine();
                    var provisionDevice = new CreateDeviceOperation(context);
                    operationParameters.Arguments.Add("deviceid", input);
                    Console.WriteLine("Do you want to auto generate Key? (Y or N) ");
                    var selection = Console.Read();
                    switch (selection)
                    {
                        case 'Y':
                            operationParameters.Arguments.Add("auto", true);
                            await provisionDevice.ExecuteAsync(operationParameters);
                            break;
                        case 'N':
                            operationParameters.Arguments.Add("auto", false);
                            //Console.WriteLine("Please enter device Key");
                            var deviceKey = "PVjPkqj3loB1P0VJzMAkIg==";
                            operationParameters.Arguments.Add("deviceKey", deviceKey);
                            await provisionDevice.ExecuteAsync(operationParameters);
                            break;
                        default:
                            Console.WriteLine("Wrong input");
                            break;
                    }
                    break;

                case SendCommand:
                    var sendCommandOperation = new SendCommandOperation(context);
                    operationParameters.Arguments["deviceid"] = "T5720022";
                    operationParameters.Arguments["commandtoexecute"] = "Command";
                    Console.WriteLine("Sending Command");
                    await sendCommandOperation.ExecuteAsync(operationParameters);
                    Console.WriteLine("Successfully sent command");
                    break;
            }
        }

        public static async Task ReadTelemetry(string deviceId, CancellationToken ct)
        {
            EventHubClient eventHubClient = null;
            EventHubReceiver eventHubReceiver = null;
            var consumerGroup = ConfigurationManager.AppSettings["ConsumerGroup"];

            try
            {
                var ioTHubConnectionString = ConfigurationManager.AppSettings["IOTHubConnectionString"];
                eventHubClient = EventHubClient.CreateFromConnectionString(ioTHubConnectionString, "messages/events");
                //var ioTHubConnectionString =
                //    "Endpoint=sb://ihsuprodhkres004dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=Rte+iSJejSjnT4pXbdbGoRd786APJGiX/5pEkk1mAU8=";
                //eventHubClient = EventHubClient.CreateFromConnectionString(ioTHubConnectionString,
                //    "iothub-ehub-raghuhub-1063-f06377c774");
                Console.WriteLine("Receiving events...\r\n");
                var eventHubPartitionsCount = eventHubClient.GetRuntimeInformation().PartitionCount;
                string partition = EventHubPartitionKeyResolver.ResolveToPartition(deviceId, eventHubPartitionsCount);
                eventHubReceiver = eventHubClient.GetConsumerGroup(consumerGroup).CreateReceiver(partition);

                while (!ct.IsCancellationRequested)
                {
                    EventData eventData = await eventHubReceiver.ReceiveAsync(TimeSpan.FromSeconds(10));

                    if (eventData != null)
                    {
                        Console.WriteLine("Received Message : ");
                        Console.WriteLine(Encoding.UTF8.GetString(eventData.GetBytes()));
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async static Task FeedbackLoop()
        {
            var feedbackReceiver = context.ServiceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }

        }
    }
}
