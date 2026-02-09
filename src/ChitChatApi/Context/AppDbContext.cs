using Npgsql;

namespace ChitChatApi.Context;

public sealed class AppDbContext
{
    private readonly NpgsqlDataSource _dataSource;

    public AppDbContext(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return _dataSource.OpenConnectionAsync(cancellationToken);
    }
}
