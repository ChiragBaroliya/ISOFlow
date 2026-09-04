using System.Data;

namespace ISOFlow.Infrastructure.Data;

/// <summary>One open connection + transaction shared across every repository call made within the current async flow.</summary>
public sealed class AmbientTransaction
{
    public IDbConnection Connection { get; }
    public IDbTransaction Transaction { get; }

    public AmbientTransaction(IDbConnection connection, IDbTransaction transaction)
    {
        Connection = connection;
        Transaction = transaction;
    }
}

/// <summary>
/// Lets a cross-cutting caller (the audit action filter) wrap a business write and its audit-log
/// insert in a single database transaction, without changing any repository method signature.
/// <see cref="BaseRepository"/> checks <see cref="Current"/> before opening a connection of its
/// own; when set, every repository call anywhere in the current request reuses this same
/// connection/transaction and skips disposing it (the scope owner disposes it once, at the end).
/// Backed by <see cref="AsyncLocal{T}"/> so it flows through the request's async call chain only —
/// it never leaks between concurrent requests.
/// </summary>
public static class AmbientDbContext
{
    private static readonly AsyncLocal<AmbientTransaction?> _current = new();

    public static AmbientTransaction? Current => _current.Value;

    /// <summary>Starts an ambient transaction scope. Dispose the result to clear it (does not close the connection).</summary>
    public static IDisposable Begin(IDbConnection connection, IDbTransaction transaction)
    {
        _current.Value = new AmbientTransaction(connection, transaction);
        return new AmbientScope();
    }

    private sealed class AmbientScope : IDisposable
    {
        public void Dispose() => _current.Value = null;
    }
}
