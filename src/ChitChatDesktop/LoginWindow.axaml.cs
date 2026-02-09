using Avalonia.Controls;
using ChitChat.ViewModels;

namespace ChitChat.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested(bool isSuccess)
    {
        Close(isSuccess);
    }
}
