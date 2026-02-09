using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChitChat.Services;

namespace ChitChat.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IChatService? _chatService;
    private readonly IEmployeeDirectoryService? _directoryService;
    private readonly CurrentUserStore? _currentUserStore;

    public MainWindowViewModel()
    {
        ChatRooms = new ObservableCollection<ChatRoomListItem>
        {
            new(1, "Проект ChitChat", DateTime.Now.AddMinutes(-12), "Обсуждаем задачи по бэкенду"),
            new(2, "UI/UX", DateTime.Now.AddHours(-2), "Нужна форма логина"),
            new(3, "Общие вопросы", DateTime.Now.AddDays(-1), "Встреча в 10:00")
        };
    }

    public MainWindowViewModel(IChatService chatService, IEmployeeDirectoryService directoryService, CurrentUserStore currentUserStore)
    {
        _chatService = chatService;
        _directoryService = directoryService;
        _currentUserStore = currentUserStore;
        ChatRooms = new ObservableCollection<ChatRoomListItem>();
    }

    public ObservableCollection<ChatRoomListItem> ChatRooms { get; }

    [ObservableProperty]
    private ChatRoomListItem? _selectedChatRoom;

    [ObservableProperty]
    private string? _errorMessage;

    public event Action<ChatRoomListItem>? OpenChatRoomRequested;
    public event Action? OpenEmployeeFinderRequested;

    [RelayCommand]
    private void OpenEmployeeFinder()
    {
        OpenEmployeeFinderRequested?.Invoke();
    }

    public void RequestOpenChatRoom(ChatRoomListItem? room)
    {
        if (room is null)
        {
            return;
        }

        OpenChatRoomRequested?.Invoke(room);
    }

    public void AddOrUpdateChatRoom(ChatRoomListItem room)
    {
        ErrorMessage = null;

        for (var i = 0; i < ChatRooms.Count; i++)
        {
            if (ChatRooms[i].Id == room.Id)
            {
                ChatRooms[i] = room;
                return;
            }
        }

        ChatRooms.Add(room);
    }

    public void RemoveChatRoom(int roomId)
    {
        ErrorMessage = null;

        for (var i = ChatRooms.Count - 1; i >= 0; i--)
        {
            if (ChatRooms[i].Id == roomId)
            {
                ChatRooms.RemoveAt(i);
                return;
            }
        }
    }

    public async Task LoadAsync()
    {
        if (_chatService is null || _currentUserStore?.CurrentEmployee is null)
        {
            ErrorMessage = "Не удалось определить текущего пользователя.";
            return;
        }

        ErrorMessage = null;

        try
        {
            var rooms = await _chatService.GetChatRoomsAsync(_currentUserStore.CurrentEmployee.Id);
            ChatRooms.Clear();
            foreach (var room in rooms)
            {
                ChatRooms.Add(new ChatRoomListItem(
                    room.Id,
                    room.Topic ?? "Без темы",
                    room.LastMessageAt,
                    room.LastMessagePreview ?? string.Empty));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить список чатов: {ex.Message}";
            Console.WriteLine($"Failed to load chat rooms: {ex}");
        }
    }

}

public sealed record ChatRoomListItem(int Id, string Topic, DateTime? LastMessageAt, string LastMessagePreview);
