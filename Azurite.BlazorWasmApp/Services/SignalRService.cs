using Azurite.SharedModels;
using Microsoft.AspNetCore.SignalR.Client;

namespace Azurite.BlazorWasmApp.Services
{
    public class SignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly List<ChatMessage> _messages = new();

        public event Action<List<ChatMessage>>? MessagesChanged;
        public event Action<string>? ConnectionStateChanged;

        public async Task StartAsync()
        {
            if (_hubConnection is not null)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8888/client/?hub=chathub")
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
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await StartAsync();
            };

            try
            {
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
