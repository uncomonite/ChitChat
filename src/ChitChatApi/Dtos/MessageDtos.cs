using System;

namespace ChitChatApi.Dtos;

public sealed record CreateMessageRequest(int SenderId, string Message);

public sealed record ChatMessageDto(int Id, int SenderId, string SenderName, int ChatRoomId, DateTime CreatedAt, string Message);
