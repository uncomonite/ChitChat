using System;
using System.Collections.Specialized;
using Avalonia.Controls;
using ChitChat.Services;
using ChitChat.ViewModels;

namespace ChitChat.Views;

public partial class ChatWindow : Window
{
    private readonly IEmployeeDirectoryService? _directoryService;

    public ChatWindow()
    {
        InitializeComponent();
    }

    public ChatWindow(ChatWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.MessageAdded += _ => ScrollToLatestMessage();
        viewModel.AddUserRequested += () => OpenEmployeeFinder(viewModel);
        viewModel.LeaveRequested += Close;

        if (viewModel.Messages is INotifyCollectionChanged notify)
        {
            notify.CollectionChanged += (_, __) => ScrollToLatestMessage();
        }
    }

    public ChatWindow(ChatWindowViewModel viewModel, IEmployeeDirectoryService directoryService) : this(viewModel)
    {
        _directoryService = directoryService;
        _ = viewModel.LoadAsync();
    }

    private void ScrollToLatestMessage()
    {
        if (MessagesList.ItemCount > 0)
        {
            MessagesList.ScrollIntoView(MessagesList.ItemCount - 1);
        }
    }

    private async void OpenEmployeeFinder(ChatWindowViewModel chatViewModel)
    {
        if (_directoryService is null)
        {
            return;
        }

        var finderViewModel = new EmployeeFinderViewModel(_directoryService);
        finderViewModel.EmployeeSelected += async employee =>
        {
            await chatViewModel.AddUserToChatAsync(employee);
        };

        var finderWindow = new EmployeeFinderWindow(finderViewModel);
        finderWindow.Show(this);
        await finderViewModel.LoadAsync();
    }
}
