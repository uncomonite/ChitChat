using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChitChat.Services;

namespace ChitChat.ViewModels;

public partial class LoginWindowViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly CredentialsStore _credentialsStore;

    public LoginWindowViewModel(IAuthService authService, CredentialsStore credentialsStore)
    {
        _authService = authService;
        _credentialsStore = credentialsStore;
    }

    public event Action<bool>? CloseRequested;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            var success = await _authService.LoginAsync(Username, Password);

            if (!success)
            {
                ErrorMessage = "Неверный логин или пароль.";
                return;
            }

            if (RememberMe)
            {
                await _credentialsStore.SaveAsync(new Credentials(Username, Password));
            }
            else
            {
                await _credentialsStore.ClearAsync();
            }

            CloseRequested?.Invoke(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(false);
    }

    public async Task LoadRememberedAsync()
    {
        var saved = await _credentialsStore.LoadAsync();
        if (saved is null)
        {
            return;
        }

        Username = saved.Username;
        Password = saved.Password;
        RememberMe = true;
    }
}
