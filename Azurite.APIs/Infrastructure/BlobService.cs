using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Azurite.APIs.Infrastructure;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "demo-container";

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzuriteStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadBlobAsync(string blobName, Stream content, string contentType = "application/octet-stream")
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true);
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadBlobAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"Blob '{blobName}' not found.");
        }

        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task<List<BlobItem>> ListBlobsAsync(string prefix = null)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobs = new List<BlobItem>();
        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
        {
            blobs.Add(blobItem);
        }

        return blobs;
    }

    public async Task<bool> DeleteBlobAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<bool> BlobExistsAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        return await blobClient.ExistsAsync();
    }
}
