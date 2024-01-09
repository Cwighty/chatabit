// <copyright file="Message.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace Chat.Data.Entities;

public partial class Message
{
  public int Id { get; set; }

  public string Message1 { get; set; } = null!;

  public DateTime CreatedAt { get; set; }
}