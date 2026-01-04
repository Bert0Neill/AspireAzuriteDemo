using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using Scalar.AspNetCore;
using Azurite.APIs.Hubs;
using Azurite.APIs.Infrastructure;
using System.Runtime.CompilerServices;
using Azure.Data.Tables;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets as the recommended local replacement for Key Vault
// Secrets can be set using: dotnet user-secrets set "KeyVault:SecretName" "value"
// Access via IConfiguration: configuration["KeyVault:SecretName"]
builder.Configuration.AddUserSecrets<Program>(optional: true);

// Add Aspire Redis distributed caching
builder.AddRedisDistributedCache("cache");

/**********************************
 *          Service Bus           *
 **********************************/
builder.AddAzureServiceBusClient("propertyContent");
var serviceBusConnectionString = builder.Configuration.GetConnectionString("sbInsurancePolicies");
string queuePropertyContentPolicy = "propertyContent";

// Register ServiceBusClient and ServiceBusSender
if (serviceBusConnectionString != null)
{
    builder.Services.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<ServiceBusClient>().CreateSender(queuePropertyContentPolicy));
}

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Register Azurite services
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<QueueService>();
builder.Services.AddScoped<VaultService>();

// Add CORS for Blazor WASM
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Azure SignalR - connection string will be injected by Aspire from the emulator reference
var signalRConnectionString = builder.Configuration.GetConnectionString("Emulator-SignalR");
if (!string.IsNullOrEmpty(signalRConnectionString))
{
    builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
}
else
{
    // Fallback to regular SignalR if connection string is not available
    builder.Services.AddSignalR();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
        .WithTitle("Your Custom Title")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

}

app.UseCors();
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};



// Service Bus endpoints
app.MapPost("/api/servicebus/send", async (ServiceBusSender sender, MessageDto message) =>
{
    var serviceBusMessage = new ServiceBusMessage(message.Text)
    {
        ContentType = "text/plain"
    };

    await sender.SendMessageAsync(serviceBusMessage);

    return Results.Ok(new { message = $"Message sent to queue '{queuePropertyContentPolicy}'" });
})
.WithName("SendServiceBusMessage")
.WithOpenApi();

// Blob Storage endpoints
app.MapPost("/api/blob/upload", async (BlobService blobService, IFormFile file) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("No file uploaded");

    using var stream = file.OpenReadStream();
    var blobUrl = await blobService.UploadBlobAsync(file.FileName, stream, file.ContentType);
    return Results.Ok(new { blobName = file.FileName, url = blobUrl });
})
.WithName("UploadBlob")
.WithOpenApi()
.Accepts<IFormFile>("multipart/form-data");

app.MapGet("/api/blob/list", async (BlobService blobService) =>
{
    var blobs = await blobService.ListBlobsAsync();
    return Results.Ok(blobs.Select(b => new { name = b.Name, size = b.Properties.ContentLength, lastModified = b.Properties.LastModified }));
})
.WithName("ListBlobs")
.WithOpenApi();

app.MapGet("/api/blob/download/{blobName}", async (BlobService blobService, string blobName) =>
{
    try
    {
        var stream = await blobService.DownloadBlobAsync(blobName);
        return Results.File(stream, "application/octet-stream", blobName);
    }
    catch (FileNotFoundException)
    {
        return Results.NotFound($"Blob '{blobName}' not found");
    }
})
.WithName("DownloadBlob")
.WithOpenApi();

app.MapDelete("/api/blob/{blobName}", async (BlobService blobService, string blobName) =>
{
    var deleted = await blobService.DeleteBlobAsync(blobName);
    return deleted ? Results.Ok(new { message = $"Blob '{blobName}' deleted" }) : Results.NotFound($"Blob '{blobName}' not found");
})
.WithName("DeleteBlob")
.WithOpenApi();

// Table Storage endpoints
app.MapPost("/api/table/entity", async (TableService tableService, TableEntityDto entity) =>
{
    var tableEntity = new TableEntity(entity.PartitionKey, entity.RowKey)
    {
        ["Data"] = entity.Data ?? ""
    };
    
    await tableService.AddEntityAsync(tableEntity);
    return Results.Ok(new { partitionKey = entity.PartitionKey, rowKey = entity.RowKey });
})
.WithName("AddTableEntity")
.WithOpenApi();

app.MapGet("/api/table/entity/{partitionKey}/{rowKey}", async (TableService tableService, string partitionKey, string rowKey) =>
{
    try
    {
        var entity = await tableService.GetEntityAsync<TableEntity>(partitionKey, rowKey);
        return Results.Ok(new { partitionKey = entity.PartitionKey, rowKey = entity.RowKey, data = entity.GetString("Data") });
    }
    catch
    {
        return Results.NotFound($"Entity not found");
    }
})
.WithName("GetTableEntity")
.WithOpenApi();

app.MapGet("/api/table/entities", async (TableService tableService, string? partitionKey = null) =>
{
    var filter = partitionKey != null ? $"PartitionKey eq '{partitionKey}'" : null;
    var entities = await tableService.QueryEntitiesAsync<TableEntity>(filter);
    return Results.Ok(entities.Select(e => new { partitionKey = e.PartitionKey, rowKey = e.RowKey, data = e.GetString("Data") }));
})
.WithName("ListTableEntities")
.WithOpenApi();

app.MapPut("/api/table/entity", async (TableService tableService, TableEntityDto entity) =>
{
    var tableEntity = new TableEntity(entity.PartitionKey, entity.RowKey)
    {
        ["Data"] = entity.Data ?? ""
    };
    
    await tableService.UpdateEntityAsync(tableEntity);
    return Results.Ok(new { partitionKey = entity.PartitionKey, rowKey = entity.RowKey });
})
.WithName("UpdateTableEntity")
.WithOpenApi();

app.MapDelete("/api/table/entity/{partitionKey}/{rowKey}", async (TableService tableService, string partitionKey, string rowKey) =>
{
    var deleted = await tableService.DeleteEntityAsync(partitionKey, rowKey);
    return deleted ? Results.Ok(new { message = "Entity deleted" }) : Results.NotFound("Entity not found");
})
.WithName("DeleteTableEntity")
.WithOpenApi();

// Queue Storage endpoints
app.MapPost("/api/queue/send", async (QueueService queueService, QueueMessageDto message) =>
{
    var messageId = await queueService.SendMessageAsync(message.Text);
    return Results.Ok(new { messageId, message = "Message sent to queue" });
})
.WithName("SendQueueMessage")
.WithOpenApi();

app.MapGet("/api/queue/receive", async (QueueService queueService) =>
{
    var message = await queueService.ReceiveMessageAsync();
    if (message == null)
        return Results.Ok(new { message = "No messages in queue" });

    return Results.Ok(new { 
        messageId = message.MessageId, 
        text = message.MessageText,
        popReceipt = message.PopReceipt
    });
})
.WithName("ReceiveQueueMessage")
.WithOpenApi();

app.MapGet("/api/queue/count", async (QueueService queueService) =>
{
    var count = await queueService.GetQueueLengthAsync();
    return Results.Ok(new { count });
})
.WithName("GetQueueLength")
.WithOpenApi();

app.MapDelete("/api/queue/message/{messageId}", async (QueueService queueService, string messageId, [FromQuery] string popReceipt) =>
{
    await queueService.DeleteMessageAsync(messageId, popReceipt);
    return Results.Ok(new { message = "Message deleted" });
})
.WithName("DeleteQueueMessage")
.WithOpenApi();

app.MapDelete("/api/queue/clear", async (QueueService queueService) =>
{
    await queueService.ClearQueueAsync();
    return Results.Ok(new { message = "Queue cleared" });
})
.WithName("ClearQueue")
.WithOpenApi();

// Key Vault endpoints
app.MapPost("/api/vault/secret", async (VaultService vaultService, SecretDto secret) =>
{
    try
    {
        var secretId = await vaultService.SetSecretAsync(secret.Name, secret.Value);
        return Results.Ok(new { name = secret.Name, id = secretId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("SetSecret")
.WithOpenApi();

app.MapGet("/api/vault/secret/{name}", async (VaultService vaultService, string name) =>
{
    var secret = await vaultService.GetSecretAsync(name);
    if (secret == null)
        return Results.NotFound(new { message = $"Secret '{name}' not found" });

    return Results.Ok(new { name, value = secret });
})
.WithName("GetSecret")
.WithOpenApi();

app.MapGet("/api/vault/secrets", async (VaultService vaultService) =>
{
    var secrets = await vaultService.ListSecretsAsync();
    return Results.Ok(secrets.Select(s => new { name = s }));
})
.WithName("ListSecrets")
.WithOpenApi();

app.MapDelete("/api/vault/secret/{name}", async (VaultService vaultService, string name) =>
{
    var deleted = await vaultService.DeleteSecretAsync(name);
    return deleted ? Results.Ok(new { message = $"Secret '{name}' deleted" }) : Results.NotFound($"Secret '{name}' not found");
})
.WithName("DeleteSecret")
.WithOpenApi();

app.MapGet("/weatherforecast", async (ServiceBusClient busClient) =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

// Map SignalR Hub
app.MapHub<MyHub>("/hubs/chat");

// Scalar API Reference and OpenAPI
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Azurite Demo API";
});

app.Run();

record MessageDto(string Text);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record TableEntityDto(string PartitionKey, string RowKey, string? Data);

record QueueMessageDto(string Text);

record SecretDto(string Name, string Value);

// Program class for User Secrets configuration
public partial class Program { }
