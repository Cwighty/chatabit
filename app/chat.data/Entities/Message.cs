using System;
using System.Collections.Generic;

namespace chat.data.Entities;

public partial class Message
{
    public int Id { get; set; }

    public string Message1 { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
