using Azure.Security.KeyVault.Secrets;
using Azure.Core;

namespace Azurite.APIs.Infrastructure;

/// <summary>
/// Key Vault Service for interacting with Azure Key Vault.
/// Note: Azure Key Vault doesn't have an official emulator like Azurite.
/// For local development, consider using Azure Key Vault Emulator or mock the service.
/// For production, use Azure.Identity with Managed Identity or Client Secret.
/// </summary>
public class VaultService
{
    private readonly SecretClient? _secretClient;
    private readonly string _vaultUrl;
    private readonly bool _useEmulator;

    public VaultService(IConfiguration configuration)
    {
        _vaultUrl = configuration.GetConnectionString("KeyVault") ?? "http://localhost:8080";
        _useEmulator = _vaultUrl.StartsWith("http://localhost") || _vaultUrl.StartsWith("http://127.0.0.1");
        
        // For emulator/local development, we'll use a simplified approach
        // In a real scenario, you'd use Azure.Identity credentials
        if (!_useEmulator)
        {
            var vaultUri = new Uri(_vaultUrl);
            // For production, use proper authentication
            // _secretClient = new SecretClient(vaultUri, new DefaultAzureCredential());
        }
    }

    // For emulator, we'll use in-memory storage as a fallback
    private static readonly Dictionary<string, string> _mockSecrets = new();

    public async Task<string> SetSecretAsync(string secretName, string secretValue)
    {
        if (_useEmulator && _secretClient == null)
        {
            // Use in-memory storage for emulator
            _mockSecrets[secretName] = secretValue;
            await Task.CompletedTask;
            return $"mock://vault/secrets/{secretName}";
        }

        try
        {
            if (_secretClient == null)
                throw new InvalidOperationException("SecretClient not initialized");

            var secret = await _secretClient.SetSecretAsync(secretName, secretValue);
            return secret.Value.Id.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to set secret: {ex.Message}", ex);
        }
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        if (_useEmulator && _secretClient == null)
        {
            // Use in-memory storage for emulator
            await Task.CompletedTask;
            return _mockSecrets.TryGetValue(secretName, out var value) ? value : null;
        }

        try
        {
            if (_secretClient == null)
                throw new InvalidOperationException("SecretClient not initialized");

            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get secret: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteSecretAsync(string secretName)
    {
        if (_useEmulator && _secretClient == null)
        {
            // Use in-memory storage for emulator
            await Task.CompletedTask;
            return _mockSecrets.Remove(secretName);
        }

        try
        {
            if (_secretClient == null)
                throw new InvalidOperationException("SecretClient not initialized");

            var operation = await _secretClient.StartDeleteSecretAsync(secretName);
            await operation.WaitForCompletionAsync();
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete secret: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListSecretsAsync()
    {
        if (_useEmulator && _secretClient == null)
        {
            // Use in-memory storage for emulator
            await Task.CompletedTask;
            return _mockSecrets.Keys.ToList();
        }

        var secrets = new List<string>();
        try
        {
            if (_secretClient == null)
                throw new InvalidOperationException("SecretClient not initialized");

            await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync())
            {
                secrets.Add(secretProperties.Name);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list secrets: {ex.Message}", ex);
        }
        return secrets;
    }
}
