using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Azurite.APIs.Infrastructure;

public class QueueService
{
    private readonly QueueServiceClient _queueServiceClient;
    private const string QueueName = "demo-queue";

    public QueueService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzuriteStorage");
        _queueServiceClient = new QueueServiceClient(connectionString);
    }

    public async Task<QueueClient> GetQueueClientAsync()
    {
        var queueClient = _queueServiceClient.GetQueueClient(QueueName);
        await queueClient.CreateIfNotExistsAsync();
        return queueClient;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        var queueClient = await GetQueueClientAsync();
        var response = await queueClient.SendMessageAsync(message);
        return response.Value.MessageId;
    }

    public async Task<QueueMessage?> ReceiveMessageAsync(TimeSpan? visibilityTimeout = null)
    {
        var queueClient = await GetQueueClientAsync();
        var response = await queueClient.ReceiveMessageAsync(visibilityTimeout: visibilityTimeout);
        return response.Value;
    }

    public async Task<List<QueueMessage>> ReceiveMessagesAsync(int maxMessages = 32, TimeSpan? visibilityTimeout = null)
    {
        var queueClient = await GetQueueClientAsync();
        var messages = new List<QueueMessage>();

        var response = await queueClient.ReceiveMessagesAsync(maxMessages: maxMessages, visibilityTimeout: visibilityTimeout);
        foreach (var message in response.Value)
        {
            messages.Add(message);
        }

        return messages;
    }

    public async Task DeleteMessageAsync(string messageId, string popReceipt)
    {
        var queueClient = await GetQueueClientAsync();
        await queueClient.DeleteMessageAsync(messageId, popReceipt);
    }

    public async Task<int> GetQueueLengthAsync()
    {
        var queueClient = await GetQueueClientAsync();
        var properties = await queueClient.GetPropertiesAsync();
        return properties.Value.ApproximateMessagesCount;
    }

    public async Task ClearQueueAsync()
    {
        var queueClient = await GetQueueClientAsync();
        await queueClient.ClearMessagesAsync();
    }
}
