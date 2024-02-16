using Microsoft.AspNetCore.SignalR;

namespace Chat.Web.Hubs;

public class ChatHub : Hub
{
    public async Task UserTyping(string user, bool isTyping)
    {
        Console.WriteLine($"User: {user} IsTyping: {isTyping}");
        await Clients.All.SendAsync("UserTyping", user, isTyping);
    }
}
