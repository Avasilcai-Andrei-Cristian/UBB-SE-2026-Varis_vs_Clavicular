using System;
using matchmaking.Domain.Enums;

namespace matchmaking.Domain.Entities;

public class Message
{
    public int MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public DateTime Timestamp { get; set; }
    public int ChatId { get; set; }
    public MessageType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public bool ShowReadReceipt { get; set; }
    public string SenderInitials { get; set; } = string.Empty;
}
