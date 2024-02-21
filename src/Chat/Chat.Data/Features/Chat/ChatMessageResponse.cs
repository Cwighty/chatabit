namespace Chat.Data.Features.Chat;

public class ChatMessageResponse
{
    public string UserName { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public int LamportTimestamp { get; set; }

    public Dictionary<string, int> VectorClock { get; set; } = new Dictionary<string, int>();

    public IEnumerable<string> Images { get; set; } = [];

    public DateTime CreatedAt { get; set; }

}
