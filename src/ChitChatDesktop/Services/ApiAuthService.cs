using System.Threading;
using System.Threading.Tasks;
using ChitChat.Dtos;

namespace ChitChat.Services;

public sealed class ApiAuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly CurrentUserStore _currentUserStore;

    public ApiAuthService(ApiClient apiClient, CurrentUserStore currentUserStore)
    {
        _apiClient = apiClient;
        _currentUserStore = currentUserStore;
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        Employee? response;

        try
        {
            response = await _apiClient.PostAsync<LoginRequest, Employee>(
                "api/auth/login",
                new LoginRequest(username, password),
                cancellationToken);
        }
        catch
        {
            _currentUserStore.Set(null);
            return false;
        }

        if (response is null)
        {
            _currentUserStore.Set(null);
            return false;
        }

        _currentUserStore.Set(response);
        return true;
    }
}
