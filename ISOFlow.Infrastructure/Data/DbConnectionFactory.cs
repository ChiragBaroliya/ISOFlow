using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ISOFlow.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateOpenConnectionAsync();
}

/// <summary>Creates PostgreSQL (Npgsql) connections.</summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Reads <c>ConnectionStrings:DefaultConnection</c>. It is never committed with credentials: supply it via
    /// User Secrets (development) or the <c>ConnectionStrings__DefaultConnection</c> environment variable.
    /// </summary>
    public DbConnectionFactory(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
    {
    }

    public DbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. Set it with " +
                "'dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Host=...;Database=...;Username=...;Password=...\"' " +
                "or the ConnectionStrings__DefaultConnection environment variable.");
        }
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IDbConnection> CreateOpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
