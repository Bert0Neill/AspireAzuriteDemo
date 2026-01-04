using System.Net.Http.Json;

namespace Azurite.BlazorWasmApp.Infrastructure;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Blob operations
    public async Task<List<BlobInfo>> ListBlobsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<BlobInfo>>("/api/blob/list");
        return response ?? new List<BlobInfo>();
    }

    public async Task<string> UploadBlobAsync(Stream content, string fileName, string contentType)
    {
        using var formData = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        formData.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("/api/blob/upload", formData);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        return result?.Url ?? "";
    }

    public async Task<bool> DeleteBlobAsync(string blobName)
    {
        var response = await _httpClient.DeleteAsync($"/api/blob/{blobName}");
        return response.IsSuccessStatusCode;
    }

    // Table operations
    public async Task<List<TableEntityInfo>> ListTableEntitiesAsync(string? partitionKey = null)
    {
        var url = partitionKey != null 
            ? $"/api/table/entities?partitionKey={Uri.EscapeDataString(partitionKey)}"
            : "/api/table/entities";
        var response = await _httpClient.GetFromJsonAsync<List<TableEntityInfo>>(url);
        return response ?? new List<TableEntityInfo>();
    }

    public async Task<bool> AddTableEntityAsync(string partitionKey, string rowKey, string data)
    {
        var entity = new { PartitionKey = partitionKey, RowKey = rowKey, Data = data };
        var response = await _httpClient.PostAsJsonAsync("/api/table/entity", entity);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTableEntityAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.DeleteAsync($"/api/table/entity/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}");
        return response.IsSuccessStatusCode;
    }

    // Queue operations
    public async Task<string> SendQueueMessageAsync(string message)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/queue/send", new { Text = message });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QueueResult>();
        return result?.MessageId ?? "";
    }

    public async Task<QueueMessageInfo?> ReceiveQueueMessageAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<QueueMessageInfo>("/api/queue/receive");
        return response;
    }

    public async Task<int> GetQueueLengthAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<QueueCountResult>("/api/queue/count");
        return response?.Count ?? 0;
    }

    public async Task<bool> DeleteQueueMessageAsync(string messageId, string popReceipt)
    {
        var response = await _httpClient.DeleteAsync($"/api/queue/message/{messageId}?popReceipt={Uri.EscapeDataString(popReceipt)}");
        return response.IsSuccessStatusCode;
    }

    public async Task ClearQueueAsync()
    {
        await _httpClient.DeleteAsync("/api/queue/clear");
    }

    // Key Vault operations
    public async Task<List<SecretInfo>> ListSecretsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<SecretInfo>>("/api/vault/secrets");
        return response ?? new List<SecretInfo>();
    }

    public async Task<bool> SetSecretAsync(string name, string value)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/vault/secret", new { Name = name, Value = value });
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetSecretAsync(string name)
    {
        var response = await _httpClient.GetAsync($"/api/vault/secret/{Uri.EscapeDataString(name)}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<SecretValueResult>();
            return result?.Value;
        }
        return null;
    }

    public async Task<bool> DeleteSecretAsync(string name)
    {
        var response = await _httpClient.DeleteAsync($"/api/vault/secret/{Uri.EscapeDataString(name)}");
        return response.IsSuccessStatusCode;
    }

    // Service Bus operations
    public async Task<string> SendServiceBusMessageAsync(string message)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/servicebus/send", new { Text = message });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ServiceBusResult>();
        return result?.Message ?? "";
    }
}

// DTOs
public record BlobInfo(string Name, long? Size, DateTimeOffset? LastModified);
public record UploadResult(string BlobName, string Url);
public record TableEntityInfo(string PartitionKey, string RowKey, string? Data);
public record QueueResult(string MessageId, string Message);
public record QueueMessageInfo(string MessageId, string Text, string PopReceipt);
public record QueueCountResult(int Count);
public record SecretInfo(string Name);
public record SecretValueResult(string Name, string Value);
public record ServiceBusResult(string Message);
