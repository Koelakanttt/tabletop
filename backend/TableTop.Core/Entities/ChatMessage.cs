namespace TableTop.Core.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
}