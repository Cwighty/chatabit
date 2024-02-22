using System.Text.Json;
using Chat.Data.Entities;

namespace Chat.Data.Features.Chat;

public static class ChatMessageExtensions
{
    public static ChatMessageResponse ToResponseModel(this ChatMessage message)
    {
        var a = JsonSerializer.Deserialize<Dictionary<string, int>>(message.VectorClock);
        return new ChatMessageResponse
        {
            UserName = message.UserName,
            MessageText = message.MessageText,
            Images = message.ChatMessageImages.Select(x => x.Id.ToString()).ToList(),
            CreatedAt = message.CreatedAt,

            VectorClock = a,
            LamportTimestamp = message.LamportClock,
        };
    }
}
