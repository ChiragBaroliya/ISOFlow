using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ISOFlow.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateOpenConnectionAsync();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly bool _isPostgres;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=isoflow;Username=postgres;Password=postgres;";
        
        // Auto-detect PostgreSQL vs SQL Server
        _isPostgres = _connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
                   || _connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase)
                   || _connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase)
                   || _connectionString.Contains("User Id=postgres", StringComparison.OrdinalIgnoreCase)
                   || !_connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase);
    }

    public DbConnectionFactory(string connectionString, bool isPostgres = true)
    {
        _connectionString = connectionString;
        _isPostgres = isPostgres;
    }

    public IDbConnection CreateConnection()
    {
        if (_isPostgres)
        {
            return new NpgsqlConnection(_connectionString);
        }
        return new SqlConnection(_connectionString);
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync()
    {
        IDbConnection connection = CreateConnection();
        if (connection is System.Data.Common.DbConnection dbConn)
        {
            await dbConn.OpenAsync();
        }
        else
        {
            connection.Open();
        }
        return connection;
    }
}
