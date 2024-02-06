using System;
using System.Collections.Generic;

namespace Chat.Data.Entities;

public partial class ImageLocation
{
    public Guid Id { get; set; }

    public Guid ChatMessageImageId { get; set; }

    public decimal ServiceIdentifier { get; set; }
}
