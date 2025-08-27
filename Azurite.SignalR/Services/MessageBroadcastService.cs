using Azurite.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace Azurite.SignalR.Services
{
    public class MessageBroadcastService : BackgroundService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessageBroadcastService> _logger;

        public MessageBroadcastService(IHubContext<ChatHub> hubContext, ILogger<MessageBroadcastService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = $"Automated message sent at {DateTime.Now:HH:mm:ss}";

                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", message, DateTime.Now, stoppingToken);
                    _logger.LogInformation("Broadcast message sent: {Message}", message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending broadcast message");
                }

                // Wait for 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}