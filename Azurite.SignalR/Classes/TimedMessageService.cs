using Azurite.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Azurite.SignalR.Classes
{
    public class TimedMessageService : BackgroundService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<TimedMessageService> _logger;

        public TimedMessageService(IHubContext<ChatHub> hubContext, ILogger<TimedMessageService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var count = 0;
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    count++;
            //    var message = $"Server tick {count} at {DateTime.Now:T}";
            //    _logger.LogInformation("Sending: {Message}", message);

            //    await _hubContext.Clients.All.SendAsync("ReceiveMessage", message, cancellationToken: stoppingToken);

            //    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            //}
        }

    }
}
