using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChitChat.Services;

namespace ChitChat.ViewModels;

public partial class ChatWindowViewModel : ViewModelBase
{
    private readonly IChatService? _chatService;
    private readonly CurrentUserStore? _currentUserStore;

    public ChatWindowViewModel() : this(new ChatRoomListItem(0, "Чат", DateTime.Now, string.Empty))
    {
    }

    public ChatWindowViewModel(ChatRoomListItem room, EmployeeListItem? directChatWith = null)
    {
        RoomId = room.Id;
        _topic = room.Topic ?? string.Empty;
        Users = directChatWith is null
            ? BuildDefaultUsers()
            : new ObservableCollection<ChatUserItem>
            {
                new(directChatWith.Id, directChatWith.Name)
            };
        Messages = BuildDefaultMessages();
    }

    public ChatWindowViewModel(ChatRoomListItem room, IChatService chatService, CurrentUserStore currentUserStore, EmployeeListItem? directChatWith = null)
    {
        _chatService = chatService;
        _currentUserStore = currentUserStore;
        RoomId = room.Id;
        _topic = room.Topic ?? string.Empty;
        Users = directChatWith is null
            ? new ObservableCollection<ChatUserItem>()
            : new ObservableCollection<ChatUserItem> { new(directChatWith.Id, directChatWith.Name) };
        Messages = new ObservableCollection<ChatMessageItem>();
    }

    public int RoomId { get; }

    public ObservableCollection<ChatUserItem> Users { get; }

    public ObservableCollection<ChatMessageItem> Messages { get; }

    [ObservableProperty]
    private string _topic;

    [ObservableProperty]
    private string _newMessageText = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public event Action<ChatMessageItem>? MessageAdded;
    public event Action? LeaveRequested;
    public event Action<int>? ChatLeft;
    public event Action? AddUserRequested;
    public event Action<string>? TopicChanged;

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(NewMessageText))
        {
            return;
        }

        var messageText = NewMessageText.Trim();

        if (_chatService is null || _currentUserStore?.CurrentEmployee is null)
        {
            var message = new ChatMessageItem("Вы", DateTime.Now, messageText);
            Messages.Add(message);
            TrimMessages();
            NewMessageText = string.Empty;
            MessageAdded?.Invoke(message);
            return;
        }

        if (RoomId == 0)
        {
            ErrorMessage = "Чат ещё не сохранён. Попробуйте открыть его заново.";
            return;
        }

        try
        {
            var created = await _chatService.SendMessageAsync(RoomId, _currentUserStore.CurrentEmployee.Id, messageText);
            var createdMessage = new ChatMessageItem(created.SenderName, created.CreatedAt, created.Message);
            Messages.Add(createdMessage);
            TrimMessages();
            NewMessageText = string.Empty;
            MessageAdded?.Invoke(createdMessage);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось отправить сообщение: {ex.Message}";
            Console.WriteLine($"Failed to send message: {ex}");

            var fallbackMessage = new ChatMessageItem("Вы", DateTime.Now, messageText);
            Messages.Add(fallbackMessage);
            TrimMessages();
            NewMessageText = string.Empty;
            MessageAdded?.Invoke(fallbackMessage);
        }
    }

    [RelayCommand]
    private void AddUser()
    {
        AddUserRequested?.Invoke();
    }

    [RelayCommand]
    private async Task ChangeTopicAsync()
    {
        ErrorMessage = null;
        Topic = Topic.Trim();

        if (_chatService is not null)
        {
            if (RoomId == 0)
            {
                ErrorMessage = "Чат ещё не сохранён. Попробуйте открыть его заново.";
                return;
            }

            try
            {
                await _chatService.UpdateTopicAsync(RoomId, Topic);
                TopicChanged?.Invoke(Topic);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не удалось изменить тему: {ex.Message}";
                Console.WriteLine($"Failed to update topic: {ex}");
            }
        }
    }

    [RelayCommand]
    private async Task LeaveAsync()
    {
        ErrorMessage = null;

        if (_chatService is null || _currentUserStore?.CurrentEmployee is null)
        {
            LeaveRequested?.Invoke();
            return;
        }

        if (RoomId == 0)
        {
            LeaveRequested?.Invoke();
            return;
        }

        try
        {
            await _chatService.RemoveMemberAsync(RoomId, _currentUserStore.CurrentEmployee.Id);
            ChatLeft?.Invoke(RoomId);
            LeaveRequested?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось выйти из чата: {ex.Message}";
            Console.WriteLine($"Failed to leave chat: {ex}");
        }
    }

    public async Task AddUserToChatAsync(EmployeeListItem employee)
    {
        ErrorMessage = null;

        if (Users.Any(u => u.Id == employee.Id))
        {
            return;
        }

        if (_chatService is not null)
        {
            if (RoomId == 0)
            {
                ErrorMessage = "Чат ещё не сохранён. Попробуйте открыть его заново.";
                return;
            }

            try
            {
                await _chatService.AddMemberAsync(RoomId, employee.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не удалось добавить участника: {ex.Message}";
                Console.WriteLine($"Failed to add member: {ex}");
                return;
            }
        }

        Users.Add(new ChatUserItem(employee.Id, employee.Name));
    }

    public async Task LoadAsync()
    {
        if (_chatService is null)
        {
            return;
        }

        ErrorMessage = null;

        try
        {
            var members = await _chatService.GetMembersAsync(RoomId);
            var memberList = members.Select(member => new ChatUserItem(member.Id, member.Name)).ToList();

            var messages = await _chatService.GetMessagesAsync(RoomId);
            var messageList = messages
                .Select(message => new ChatMessageItem(message.SenderName, message.CreatedAt, message.Message))
                .ToList();

            Users.Clear();
            foreach (var member in memberList)
            {
                Users.Add(member);
            }

            Messages.Clear();
            foreach (var message in messageList)
            {
                Messages.Add(message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить чат: {ex.Message}";
            Console.WriteLine($"Failed to load chat data: {ex}");
        }
    }

    private void TrimMessages()
    {
        while (Messages.Count > 100)
        {
            Messages.RemoveAt(0);
        }
    }

    private static ObservableCollection<ChatUserItem> BuildDefaultUsers() => new()
    {
        new(1, "Martin Baumann"),
        new(2, "Artan Shala"),
        new(3, "Marry Hoegart")
    };

    private static ObservableCollection<ChatMessageItem> BuildDefaultMessages() => new()
    {
        new("Martin Baumann", DateTime.Now.AddMinutes(-12), "Доброе утро!"),
        new("Artan Shala", DateTime.Now.AddMinutes(-10), "Привет!"),
        new("Marry Hoegart", DateTime.Now.AddMinutes(-8), "Есть новости по задаче?")
    };
}

public sealed record ChatMessageItem(string SenderName, DateTime SentAt, string Text);

public sealed record ChatUserItem(int Id, string Name);
