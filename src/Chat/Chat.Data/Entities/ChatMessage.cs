using System;
using System.Collections.Generic;

namespace Chat.Data.Entities;

public partial class ChatMessage
{
    public Guid Id { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UserName { get; set; } = null!;

    public int LamportClock { get; set; }

    public string VectorClock { get; set; } = null!;

    public virtual ICollection<ChatMessageImage> ChatMessageImages { get; set; } = new List<ChatMessageImage>();
}
