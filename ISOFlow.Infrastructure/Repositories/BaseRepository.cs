using System.Data;
using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

/// <summary>
/// Base repository that centralizes connection lifecycle management, Dapper execution, and pagination queries.
/// When an ambient transaction is active (see <see cref="AmbientDbContext"/>) every helper below transparently
/// reuses that shared connection/transaction instead of opening its own — this is what lets a business write
/// and its audit-log insert commit or roll back together without any repository method taking a transaction
/// parameter.
/// </summary>
public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory DbConnectionFactory;

    protected BaseRepository(IDbConnectionFactory dbConnectionFactory)
    {
        DbConnectionFactory = dbConnectionFactory;
    }

    /// <summary>Resolves the connection to use for one call: the ambient one if a scope is active, else a fresh one.</summary>
    private (IDbConnection Connection, IDbTransaction? Transaction, bool OwnsConnection) ResolveConnection()
    {
        var ambient = AmbientDbContext.Current;
        return ambient != null
            ? (ambient.Connection, ambient.Transaction, false)
            : (DbConnectionFactory.CreateConnection(), null, true);
    }

    protected async Task<List<T>> QueryListAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            var result = await conn.QueryAsync<T>(sql, param, transaction: tx, commandType: commandType);
            return result.ToList();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<List<T>> QueryMappedListAsync<T>(string sql, Func<dynamic, T> mapper, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            var rows = await conn.QueryAsync<dynamic>(sql, param, transaction: tx, commandType: commandType);
            return rows.Select(mapper).ToList();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            return await conn.QueryFirstOrDefaultAsync<T>(sql, param, transaction: tx, commandType: commandType);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<T?> QueryMappedFirstOrDefaultAsync<T>(string sql, Func<dynamic, T> mapper, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, param, transaction: tx, commandType: commandType);
            return row != null ? mapper(row) : default;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<T> QuerySingleAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            return await conn.QuerySingleAsync<T>(sql, param, transaction: tx, commandType: commandType);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            return await conn.QuerySingleOrDefaultAsync<T>(sql, param, transaction: tx, commandType: commandType);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            return await conn.ExecuteAsync(sql, param, transaction: tx, commandType: commandType);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            return await conn.ExecuteScalarAsync<T>(sql, param, transaction: tx, commandType: commandType);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task<PagedResponse<T>> QueryPagedAsync<T>(
        string sql,
        Func<dynamic, T> mapper,
        DynamicParameters parameters,
        int pageNumber,
        int pageSize)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            var rows = (await conn.QueryAsync<dynamic>(sql, parameters, transaction: tx)).ToList();
            if (!rows.Any())
            {
                return new PagedResponse<T>(new List<T>(), 0, pageNumber, pageSize);
            }

            var totalCount = (int)(rows.First().total_count ?? 0);
            var items = rows.Select(mapper).ToList();

            return new PagedResponse<T>(items, totalCount, pageNumber, pageSize);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    /// <summary>
    /// Hands the caller the raw connection (and, if an ambient transaction is active, that transaction too)
    /// for custom multi-statement work. Pass <paramref name="action"/>'s transaction argument to any Dapper
    /// call you make so it participates in the ambient audit transaction when one is active.
    /// </summary>
    protected async Task<TResult> WithConnectionAsync<TResult>(Func<IDbConnection, IDbTransaction?, Task<TResult>> action)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            return await action(conn, tx);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    protected async Task WithConnectionAsync(Func<IDbConnection, IDbTransaction?, Task> action)
    {
        var (conn, tx, owns) = ResolveConnection();
        try
        {
            await action(conn, tx);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
