using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Azurite.SignalR.Hubs
{

    public class ChatHub : Hub
    {
        public async Task SendMessageToAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System",
                $"{Context.ConnectionId} joined {groupName}", DateTime.Now);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System",
                $"{Context.ConnectionId} left {groupName}", DateTime.Now);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveMessage", "System",
                $"User {Context.ConnectionId} connected", DateTime.Now);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("ReceiveMessage", "System",
                $"User {Context.ConnectionId} disconnected", DateTime.Now);
            await base.OnDisconnectedAsync(exception);
        }
    }
}