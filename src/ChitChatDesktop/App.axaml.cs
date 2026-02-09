using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ChitChat.Services;
using ChitChat.ViewModels;
using ChitChat.Views;
using System;
using System.IO;
using System.Linq;

namespace ChitChat;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var credentialsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ChitChat",
                "credentials.json");

            var credentialsStore = new CredentialsStore(credentialsPath);
            var currentUserStore = new CurrentUserStore();
            var apiBaseUrl = Environment.GetEnvironmentVariable("CHITCHAT_API_URL") ?? "http://localhost:5000";
            var apiClient = new ApiClient(apiBaseUrl);
            var authService = new ApiAuthService(apiClient, currentUserStore);
            var chatService = new ChatService(apiClient);
            var directoryService = new EmployeeDirectoryService(apiClient);
            var loginViewModel = new LoginWindowViewModel(authService, credentialsStore);

            loginViewModel.CloseRequested += success =>
            {
                if (!success)
                {
                    desktop.Shutdown();
                    return;
                }

                var mainWindow = new MainWindow(chatService, directoryService, currentUserStore)
                {
                    DataContext = new MainWindowViewModel(chatService, directoryService, currentUserStore),
                };

                desktop.MainWindow = mainWindow;
                mainWindow.Show();
            };

            var loginWindow = new LoginWindow(loginViewModel);
            desktop.MainWindow = loginWindow;

            _ = loginViewModel.LoadRememberedAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
