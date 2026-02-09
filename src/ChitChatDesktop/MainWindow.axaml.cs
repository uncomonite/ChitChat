using System;
using Avalonia.Controls;
using Avalonia.Input;
using ChitChat.Services;
using ChitChat.ViewModels;

namespace ChitChat.Views;

public partial class MainWindow : Window
{
    private readonly IChatService? _chatService;
    private readonly IEmployeeDirectoryService? _directoryService;
    private readonly CurrentUserStore? _currentUserStore;

    public MainWindow()
    {
        InitializeComponent();
        ChatRoomsList.DoubleTapped += OnChatRoomDoubleTapped;
    }

    public MainWindow(IChatService chatService, IEmployeeDirectoryService directoryService, CurrentUserStore currentUserStore)
        : this()
    {
        _chatService = chatService;
        _directoryService = directoryService;
        _currentUserStore = currentUserStore;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.OpenChatRoomRequested += OnOpenChatRoomRequested;
            viewModel.OpenEmployeeFinderRequested += OnOpenEmployeeFinderRequested;
            _ = viewModel.LoadAsync();
        }
    }

    private void OnChatRoomDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.RequestOpenChatRoom(viewModel.SelectedChatRoom);
        }
    }

    private void OnOpenChatRoomRequested(ChatRoomListItem room)
    {
        ChatWindow chatWindow;

        if (_chatService is not null && _currentUserStore is not null && _directoryService is not null)
        {
            var chatViewModel = new ChatWindowViewModel(room, _chatService, _currentUserStore);
            if (DataContext is MainWindowViewModel viewModel)
            {
                chatViewModel.TopicChanged += topic =>
                    viewModel.AddOrUpdateChatRoom(new ChatRoomListItem(
                        room.Id,
                        topic,
                        room.LastMessageAt,
                        room.LastMessagePreview));
                chatViewModel.ChatLeft += roomId => viewModel.RemoveChatRoom(roomId);
            }

            chatWindow = new ChatWindow(chatViewModel, _directoryService);
            _ = chatViewModel.LoadAsync();
        }
        else
        {
            var chatViewModel = new ChatWindowViewModel(room);
            chatWindow = new ChatWindow(chatViewModel);
        }

        chatWindow.Show();
    }

    private async void OnOpenEmployeeFinderRequested()
    {
        if (_chatService is null || _directoryService is null || _currentUserStore?.CurrentEmployee is null)
        {
            var viewModel = new EmployeeFinderViewModel();
            var fallbackWindow = new EmployeeFinderWindow(viewModel);
            fallbackWindow.Show();
            return;
        }

        var currentUser = _currentUserStore.CurrentEmployee;
        var directoryViewModel = new EmployeeFinderViewModel(_directoryService);
        var finderWindow = new EmployeeFinderWindow(directoryViewModel);
        directoryViewModel.EmployeeSelected += async employee =>
        {
            try
            {
                var room = await _chatService.CreateChatRoomAsync($"Чат с {employee.Name}", currentUser.Id);
                await _chatService.AddMemberAsync(room.Id, employee.Id);

                var roomItem = new ChatRoomListItem(room.Id, room.Topic ?? $"Чат с {employee.Name}", null, string.Empty);
                var chatViewModel = new ChatWindowViewModel(roomItem, _chatService, _currentUserStore, employee);
                if (DataContext is MainWindowViewModel viewModel)
                {
                    chatViewModel.TopicChanged += topic =>
                        viewModel.AddOrUpdateChatRoom(new ChatRoomListItem(
                            roomItem.Id,
                            topic,
                            roomItem.LastMessageAt,
                            roomItem.LastMessagePreview));
                    chatViewModel.ChatLeft += roomId => viewModel.RemoveChatRoom(roomId);
                }

                var chatWindow = new ChatWindow(chatViewModel, _directoryService);
                chatWindow.Show();

                if (DataContext is MainWindowViewModel mainViewModel)
                {
                    mainViewModel.AddOrUpdateChatRoom(roomItem);
                    await mainViewModel.LoadAsync();
                }
            }
            catch (Exception ex)
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.ErrorMessage = $"Не удалось создать чат: {ex.Message}";
                }

                Console.WriteLine($"Failed to create chat room: {ex}");
            }
        };

        finderWindow.Show(this);
        await directoryViewModel.LoadAsync();
    }
}
