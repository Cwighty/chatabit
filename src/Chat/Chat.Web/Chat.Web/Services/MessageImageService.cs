using Chat.Data.Entities;

namespace Chat.Web.Services;

public interface IMessageImageService
{
    Task<IEnumerable<ChatMessageImage>?> GetMessages();
}

public class MessageImageService : IMessageImageService
{
    private HttpClient httpClient;

    public MessageImageService(IHttpClientFactory httpClientFactory)
    {
        this.httpClient = httpClientFactory.CreateClient("ImageProcessing");
    }

    public async Task<IEnumerable<ChatMessageImage>?> GetMessages()
    {
        return await httpClient.GetFromJsonAsync<List<ChatMessageImage>>("/api/Image");
    }
}
