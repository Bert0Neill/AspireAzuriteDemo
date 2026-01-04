using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace Azurite.APIs.Infrastructure;

public class TableService
{
    private readonly TableServiceClient _tableServiceClient;
    private const string TableName = "DemoTable";

    public TableService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzuriteStorage");
        _tableServiceClient = new TableServiceClient(connectionString);
    }

    public async Task<TableClient> GetTableClientAsync()
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();
        return tableClient;
    }

    public async Task<T> AddEntityAsync<T>(T entity) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync();
        await tableClient.AddEntityAsync(entity);
        return entity;
    }

    public async Task<T> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync();
        var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
        return response.Value;
    }

    public async Task<T> UpdateEntityAsync<T>(T entity) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync();
        await tableClient.UpdateEntityAsync(entity, ETag.All);
        return entity;
    }

    public async Task<bool> DeleteEntityAsync(string partitionKey, string rowKey)
    {
        var tableClient = await GetTableClientAsync();
        try
        {
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<T>> QueryEntitiesAsync<T>(string filter = null) where T : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync();
        var entities = new List<T>();

        var query = filter != null 
            ? tableClient.QueryAsync<T>(filter) 
            : tableClient.QueryAsync<T>();

        await foreach (var entity in query)
        {
            entities.Add(entity);
        }

        return entities;
    }
}
