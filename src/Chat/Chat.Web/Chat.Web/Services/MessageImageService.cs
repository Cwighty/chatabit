using Chat.Data;
using Chat.Data.Entities;
using Chat.Data.Features.Chat;
using Microsoft.EntityFrameworkCore;

namespace Chat.Web.Services;

public interface IMessageImageService
{
    Task<IEnumerable<ChatMessageImage>?> GetMessages();
}

public class MessageImageService(HttpClient httpClient) : IMessageImageService
{
    public async Task<IEnumerable<ChatMessageImage>?> GetMessages()
    {
        return await httpClient.GetFromJsonAsync<List<ChatMessageImage>>("/api/Image");
    }
}
