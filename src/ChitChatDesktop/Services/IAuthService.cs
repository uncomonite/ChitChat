using System.Threading;
using System.Threading.Tasks;

namespace ChitChat.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
