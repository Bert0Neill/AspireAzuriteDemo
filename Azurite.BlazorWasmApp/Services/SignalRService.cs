using Azurite.SharedModels;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using System.Text.Json;

namespace Azurite.BlazorWasmApp.Services
{
    public class SignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly List<ChatMessage> _messages = new();
        private readonly HttpClient _httpClient;

        public event Action<List<ChatMessage>>? MessagesChanged;
        public event Action<string>? ConnectionStateChanged;

        public SignalRService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task StartAsync(string? userId = null)
        {
            if (_hubConnection is not null)
                return;

            try
            {
                // Get negotiate response from server
                var negotiateRequest = new { UserId = userId ?? "anonymous" };
                var negotiateResponse = await _httpClient.PostAsJsonAsync("/api/signalr/negotiate", negotiateRequest);

                if (!negotiateResponse.IsSuccessStatusCode)
                {
                    ConnectionStateChanged?.Invoke($"Negotiate failed: {negotiateResponse.StatusCode}");
                    return;
                }

                var negotiateContent = await negotiateResponse.Content.ReadAsStringAsync();
                var negotiateData = JsonSerializer.Deserialize<NegotiateResponse>(negotiateContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (negotiateData?.Url == null)
                {
                    ConnectionStateChanged?.Invoke("Invalid negotiate response");
                    return;
                }

                // Create connection with the URL from negotiate
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(negotiateData.Url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult<string?>(negotiateData.AccessToken);
                    })
                    .Build();

                _hubConnection.On<string, string, DateTime>("ReceiveMessage", (user, message, timestamp) =>
                {
                    var chatMessage = new ChatMessage
                    {
                        User = user,
                        Message = message,
                        Timestamp = timestamp
                    };

                    _messages.Insert(0, chatMessage); // Add to beginning for newest first

                    // Keep only last 100 messages
                    if (_messages.Count > 100)
                        _messages.RemoveAt(_messages.Count - 1);

                    MessagesChanged?.Invoke(_messages.ToList());
                });

                _hubConnection.Closed += async (error) =>
                {
                    ConnectionStateChanged?.Invoke("Disconnected");
                    // Wait and try to reconnect
                    await Task.Delay(5000);
                    _ = Task.Run(async () => await StartAsync(userId));
                };

                await _hubConnection.StartAsync();
                ConnectionStateChanged?.Invoke("Connected");
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke($"Connection failed: {ex.Message}");
            }
        }

        public async Task SendMessageAsync(string user, string message)
        {
            if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("SendMessageToAll", user, message);
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                ConnectionStateChanged?.Invoke("Disconnected");
            }
        }

        public string GetConnectionState()
        {
            return _hubConnection?.State.ToString() ?? "Disconnected";
        }

        public List<ChatMessage> GetMessages() => _messages.ToList();

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}

    public class NegotiateResponse
    {
        public string? Url { get; set; }
        public string? AccessToken { get; set; }
    }