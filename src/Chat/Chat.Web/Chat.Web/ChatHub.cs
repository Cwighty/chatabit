using Microsoft.AspNetCore.SignalR;

namespace Chat.Web.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("UserTyping", user, message);
    }
}