using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Azurite.AzFnx_MonitorServicebus
{
    public class ServiceBusFunction_Queue
    {
        private readonly ILogger<ServiceBusFunction_Queue> _logger;

        public ServiceBusFunction_Queue(ILogger<ServiceBusFunction_Queue> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusFunction_Queue))]
        public async Task Run(
            [ServiceBusTrigger("myqueue", Connection = "myservicebus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
