using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChitChat.Services;

public sealed class DummyAuthService : IAuthService
{
    public Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var isValid = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
        return Task.FromResult(isValid);
    }
}
