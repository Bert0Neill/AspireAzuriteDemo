using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppServiceBusClient.Services
{
    public class DemoPolicyBusService : BackgroundService
    {

        private readonly ServiceBusClient _client;
        const string queueName = "DemoPoliciesQueue";
        private readonly ILogger<DemoPolicyBusService> _logger;
        public DemoPolicyBusService(ServiceBusClient client, ILogger<DemoPolicyBusService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task ReceiveMessageAsync()
        {
            //var processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            //{
            //    MaxConcurrentCalls = 1,
            //    AutoCompleteMessages = false
            //});

            //var processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                //    processor.ProcessMessageAsync += MessageHandler;
                //    processor.ProcessErrorAsync += ErrorHandler;

                //    await processor.StartProcessingAsync();
                //    Console.WriteLine("Receiving messages. Press key to stop");
                //    Console.ReadKey();

                //    await processor.StopProcessingAsync();
            }
            finally {          
               // await processor.DisposeAsync();
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError($"Error: {args.Exception}");
            return Task.CompletedTask;
        }

        private Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation($"Received message: {body}");
            Console.WriteLine($"Received new message: {body}");

            return args.CompleteMessageAsync(args.Message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ReceiveMessageAsync();

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    // Simulate some work
            //    Console.WriteLine("DemoPolicyBusService is running.");
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }
}
