namespace Chat.Data.Entities;

public partial class ChatMessage
{
    public int Id { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UserName { get; set; } = null!;
}
