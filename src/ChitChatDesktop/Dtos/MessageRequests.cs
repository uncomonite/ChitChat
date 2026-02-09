namespace ChitChat.Dtos;

public sealed record CreateMessageRequest(int SenderId, string Message);
