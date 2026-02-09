using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChitChat.Dtos;

namespace ChitChat.Services;

public interface IChatService
{
    Task<IReadOnlyList<Chat>> GetChatRoomsAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatRoomMember>> GetMembersAsync(int chatRoomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(int chatRoomId, CancellationToken cancellationToken = default);

    Task<Chat> CreateChatRoomAsync(string? topic, int creatorEmployeeId, CancellationToken cancellationToken = default);

    Task AddMemberAsync(int chatRoomId, int employeeId, CancellationToken cancellationToken = default);

    Task RemoveMemberAsync(int chatRoomId, int employeeId, CancellationToken cancellationToken = default);

    Task UpdateTopicAsync(int chatRoomId, string? topic, CancellationToken cancellationToken = default);

    Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string message, CancellationToken cancellationToken = default);
}
