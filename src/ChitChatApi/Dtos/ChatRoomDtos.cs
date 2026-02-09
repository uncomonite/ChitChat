using System;

namespace ChitChatApi.Dtos;

public sealed record ChatRoomDto(int Id, string? Topic, DateTime? LastMessageAt, string? LastMessagePreview);

public sealed record CreateChatRoomRequest(string? Topic, int CreatorEmployeeId);

public sealed record AddChatRoomMemberRequest(int EmployeeId);

public sealed record UpdateChatRoomTopicRequest(string? Topic);

public sealed record ChatRoomMemberDto(int Id, string Name, int? DepartmentId, string Username, DateTime JoinedAt);
