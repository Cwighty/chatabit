using System.Net.Http.Json;
using Chat.Data.Features.Chat;

namespace Chat.Web.Client;

public class MessageFetcher
{
    private readonly HttpClient _httpClient;

    public MessageFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ChatMessageResponse>> FetchMessages()
    {
        var messages = await _httpClient.GetFromJsonAsync<List<ChatMessageResponse>>("api/chat");
        return messages!;
    }

    public async Task<List<ChatMessageResponse>> FetchMessages(DateTime lastMessageDate)
    {
        var messages = await _httpClient.GetFromJsonAsync<List<ChatMessageResponse>>($"api/chat?lastMessageDate={lastMessageDate}");
        return messages!;
    }
}
