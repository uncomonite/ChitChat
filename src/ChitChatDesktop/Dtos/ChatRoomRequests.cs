namespace ChitChat.Dtos;

public sealed record CreateChatRoomRequest(string? Topic, int CreatorEmployeeId);

public sealed record AddChatRoomMemberRequest(int EmployeeId);

public sealed record UpdateChatRoomTopicRequest(string? Topic);
