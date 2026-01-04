using Microsoft.AspNetCore.SignalR;

namespace Azurite.APIs.Hubs
{
    public class MyHub : Hub
    {
        // Method clients can call to send messages to all
        public async Task SendMessageToAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.UtcNow);
        }

        // Method clients can call to send messages
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.UtcNow);
        }
    }
}
