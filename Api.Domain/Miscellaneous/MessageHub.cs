using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Domain.Miscellaneous
{
    [Authorize]
    public class MessageHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync($"Connection ID {Context.ConnectionId} is connected");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync($"Connection ID {Context.ConnectionId} is disconnected");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync(nameof(AddToGroup), $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync(nameof(RemoveFromGroup), $"{Context.ConnectionId} has left the group {groupName}.");
        }

        public async Task SendMessageToId(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync(nameof(SendMessageToId), message);
        }

        public async Task SendMessageToUser(string user, string message)
        {
            await Clients.All.SendAsync(nameof(SendMessageToUser), user, message);
        }

        public async Task SendMessageToGroup(string group, string user, string message)
        {
            await Clients.Group(group).SendAsync(nameof(SendMessageToGroup), user, message);
        }
    }
}
