using Chat.Data.Entities;

namespace Chat.Data.Features.Chat;

public static class ChatMessageExtensions
{
    public static ChatMessageResponse ToResponseModel(this ChatMessage message, IEnumerable<string> images)
    {
        return new ChatMessageResponse
        {
            UserName = message.UserName,
            MessageText = message.MessageText,
            Images = images,
        };
    }
}
