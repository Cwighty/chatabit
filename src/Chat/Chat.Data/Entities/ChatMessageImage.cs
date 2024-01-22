using System;
using System.Collections.Generic;

namespace Chat.Data.Entities;

public partial class ChatMessageImage
{
    public int Id { get; set; }

    public int ChatMessageId { get; set; }

    public byte[] ImageData { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public virtual ChatMessage ChatMessage { get; set; } = null!;
}
