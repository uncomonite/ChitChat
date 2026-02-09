using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ChitChat.Services;

public sealed class CredentialsStore
{
    private readonly string _filePath;

    public CredentialsStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task SaveAsync(Credentials credentials, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, credentials, cancellationToken: cancellationToken);
    }

    public async Task<Credentials?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<Credentials>(stream, cancellationToken: cancellationToken);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }

        return Task.CompletedTask;
    }
}

public sealed record Credentials(string Username, string Password);
