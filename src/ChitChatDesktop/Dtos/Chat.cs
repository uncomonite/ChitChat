using System;

namespace ChitChat.Dtos;

public sealed record Chat(int Id, string? Topic, DateTime? LastMessageAt, string? LastMessagePreview);
