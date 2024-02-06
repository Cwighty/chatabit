using System;
using System.Collections.Generic;

namespace Chat.Data.Entities;

public partial class ChatMessageImage
{
    public Guid Id { get; set; }

    public Guid ChatMessageId { get; set; }

    public virtual ChatMessage ChatMessage { get; set; } = null!;
}
