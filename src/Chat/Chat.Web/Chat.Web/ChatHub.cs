using Microsoft.AspNetCore.SignalR;

namespace Chat.Web.Hubs;

public class ChatHub : Hub
{
    public async Task UserTyping(string user, bool isTyping)
    {
        await Clients.Others.SendAsync("UserTyping", user, isTyping);
    }
}
