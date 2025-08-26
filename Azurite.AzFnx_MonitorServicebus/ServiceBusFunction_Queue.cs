using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Azurite.AzFnx_MonitorServicebus
{
    public class ServiceBusFunction_Queue
    {
        private readonly ILogger<ServiceBusFunction_Queue> _logger;
        private readonly IServiceManager _serviceManager;

        public ServiceBusFunction_Queue(ILogger<ServiceBusFunction_Queue> logger)
        {
            _logger = logger;

           
        }

        [Function(nameof(ServiceBusFunction_Queue))]
        public async Task Run(
            [ServiceBusTrigger("propertyContent", Connection = "sbInsurancePolicies")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions            
            )
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);


           

            // Push to SignalR clients
            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Body.ToString());

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
