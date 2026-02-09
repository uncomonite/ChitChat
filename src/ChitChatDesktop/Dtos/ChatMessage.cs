using System;

namespace ChitChat.Dtos;

public sealed record ChatMessage(int Id, int SenderId, string SenderName, int ChatRoomId, DateTime CreatedAt, string Message);
