using Microsoft.AspNetCore.SignalR;

namespace Azurite.APIs.Hubs
{
    public class MyHub : Hub
    {
        // Example: a simple method clients can call
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
