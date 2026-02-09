using System;

namespace ChitChat.Dtos;

public sealed record ChatRoomMember(int Id, string Name, int? DepartmentId, string Username, DateTime JoinedAt);
