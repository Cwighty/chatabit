using System.Net.Http.Json;
using Chat.Data.Features.Chat;

namespace Chat.Web.Client;

public class MessageFetcher
{
    private readonly HttpClient _httpClient;
    private Timer? _timer;
    public event Action<List<ChatMessageResponse>>? OnMessagesUpdated;

    public MessageFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void StartFetchingMessages(TimeSpan interval)
    {
        _timer = new Timer(async _ => await FetchMessages(), null, TimeSpan.Zero, interval);
    }

    public async Task FetchMessages()
    {
        try
        {
            var messages = await _httpClient.GetFromJsonAsync<List<ChatMessageResponse>>("api/chat");
            OnMessagesUpdated?.Invoke(messages);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
