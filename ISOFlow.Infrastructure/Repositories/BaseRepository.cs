using System.Data;
using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

/// <summary>
/// Base repository that centralizes connection lifecycle management, Dapper execution, and pagination queries.
/// </summary>
public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory DbConnectionFactory;

    protected BaseRepository(IDbConnectionFactory dbConnectionFactory)
    {
        DbConnectionFactory = dbConnectionFactory;
    }

    /// <summary>
    /// Executes a query and returns a list of results.
    /// </summary>
    protected async Task<List<T>> QueryListAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        var result = await conn.QueryAsync<T>(sql, param, commandType: commandType);
        return result.ToList();
    }

    /// <summary>
    /// Executes a dynamic query and maps each row via a custom mapper function.
    /// </summary>
    protected async Task<List<T>> QueryMappedListAsync<T>(string sql, Func<dynamic, T> mapper, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(sql, param, commandType: commandType);
        return rows.Select(mapper).ToList();
    }

    /// <summary>
    /// Executes a query and returns the first result or default.
    /// </summary>
    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<T>(sql, param, commandType: commandType);
    }

    /// <summary>
    /// Executes a query and maps the first dynamic row or returns null.
    /// </summary>
    protected async Task<T?> QueryMappedFirstOrDefaultAsync<T>(string sql, Func<dynamic, T> mapper, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, param, commandType: commandType);
        return row != null ? mapper(row) : default;
    }

    /// <summary>
    /// Executes a query that returns a single scalar value.
    /// </summary>
    protected async Task<T> QuerySingleAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        return await conn.QuerySingleAsync<T>(sql, param, commandType: commandType);
    }

    /// <summary>
    /// Executes a query that returns a single scalar value or default.
    /// </summary>
    protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(sql, param, commandType: commandType);
    }

    /// <summary>
    /// Executes an INSERT, UPDATE, or DELETE command and returns the number of affected rows.
    /// </summary>
    protected async Task<int> ExecuteAsync(string sql, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        return await conn.ExecuteAsync(sql, param, commandType: commandType);
    }

    /// <summary>
    /// Executes a scalar query.
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<T>(sql, param, commandType: commandType);
    }

    /// <summary>
    /// Executes a database-paged stored procedure/function that returns total_count as part of the row.
    /// </summary>
    protected async Task<PagedResponse<T>> QueryPagedAsync<T>(
        string sql,
        Func<dynamic, T> mapper,
        DynamicParameters parameters,
        int pageNumber,
        int pageSize)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        var rows = (await conn.QueryAsync<dynamic>(sql, parameters)).ToList();
        if (!rows.Any())
        {
            return new PagedResponse<T>(new List<T>(), 0, pageNumber, pageSize);
        }

        var totalCount = (int)(rows.First().total_count ?? 0);
        var items = rows.Select(mapper).ToList();

        return new PagedResponse<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Executes custom operations with an open connection callback.
    /// </summary>
    protected async Task<TResult> WithConnectionAsync<TResult>(Func<IDbConnection, Task<TResult>> action)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        return await action(conn);
    }

    /// <summary>
    /// Executes custom operations with an open connection callback without return value.
    /// </summary>
    protected async Task WithConnectionAsync(Func<IDbConnection, Task> action)
    {
        using var conn = DbConnectionFactory.CreateConnection();
        await action(conn);
    }
}
