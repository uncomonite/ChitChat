using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChitChat.Dtos;

namespace ChitChat.Services;

public sealed class ChatService : IChatService
{
    private readonly ApiClient _apiClient;

    public ChatService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<Chat>> GetChatRoomsAsync(int employeeId, CancellationToken cancellationToken = default)
        => await _apiClient.GetAsync<List<Chat>>($"api/chatrooms?employeeId={employeeId}", cancellationToken)
           ?? new List<Chat>();

    public async Task<IReadOnlyList<ChatRoomMember>> GetMembersAsync(int chatRoomId, CancellationToken cancellationToken = default)
        => await _apiClient.GetAsync<List<ChatRoomMember>>($"api/chatrooms/{chatRoomId}/members", cancellationToken)
           ?? new List<ChatRoomMember>();

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(int chatRoomId, CancellationToken cancellationToken = default)
        => await _apiClient.GetAsync<List<ChatMessage>>($"api/chatrooms/{chatRoomId}/messages", cancellationToken)
           ?? new List<ChatMessage>();

    public async Task<Chat> CreateChatRoomAsync(string? topic, int creatorEmployeeId, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync<CreateChatRoomRequest, Chat>(
               "api/chatrooms",
               new CreateChatRoomRequest(topic, creatorEmployeeId),
               cancellationToken)
           ?? new Chat(0, topic, null, null);

    public Task AddMemberAsync(int chatRoomId, int employeeId, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync($"api/chatrooms/{chatRoomId}/members", new AddChatRoomMemberRequest(employeeId), cancellationToken);

    public Task RemoveMemberAsync(int chatRoomId, int employeeId, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync($"api/chatrooms/{chatRoomId}/members/{employeeId}", cancellationToken);

    public Task UpdateTopicAsync(int chatRoomId, string? topic, CancellationToken cancellationToken = default)
        => _apiClient.PatchAsync($"api/chatrooms/{chatRoomId}/topic", new UpdateChatRoomTopicRequest(topic), cancellationToken);

    public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string message, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync<CreateMessageRequest, ChatMessage>(
               $"api/chatrooms/{chatRoomId}/messages",
               new CreateMessageRequest(senderId, message),
               cancellationToken)
           ?? new ChatMessage(0, senderId, string.Empty, chatRoomId, default, message);
}
