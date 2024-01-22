namespace Chat.Data.Features.Chat;

public class ChatMessageResponse
{
    public string UserName { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public IEnumerable<byte[]> Images { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }

}
