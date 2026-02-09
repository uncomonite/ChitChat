namespace ChitChatApi.Models;

public sealed class ChatMessage
{
    public int Id { get; init; }
    public int SenderId { get; init; }
    public int ChatRoomId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Message { get; init; } = string.Empty;
}
