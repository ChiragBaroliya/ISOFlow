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

    // ── JSON list columns ───────────────────────────────────────────────────────────────────────
    // Several tables store string lists as JSON TEXT columns (e.g. policies.linked_control_ids_json = '["CTRL-001"]').
    // The CRUD stored procedures don't carry those columns, so repositories persist and hydrate them with the two
    // helpers below, right after the procedure call.

    /// <summary>Maps one JSON TEXT column to a string-list property of <typeparamref name="T"/>.</summary>
    protected sealed record JsonListColumn<T>(string Column, Func<T, List<string>> Get, Action<T, List<string>> Set);

    private static readonly System.Text.RegularExpressions.Regex SqlIdentifier = new("^[a-z_]+$");

    private static string CheckIdentifier(string name) =>
        SqlIdentifier.IsMatch(name) ? name : throw new ArgumentException($"'{name}' is not a valid SQL identifier.", nameof(name));

    /// <summary>
    /// Writes the entity's list properties to their JSON columns. <paramref name="organizationId"/> scopes the
    /// update to one tenant; pass null only for tables without an organization_id column. Set
    /// <paramref name="idMayBeCode"/> for tables whose update methods also accept the row's code as its id.
    /// </summary>
    protected async Task SaveJsonListsAsync<T>(string table, string id, int? organizationId, T entity, IReadOnlyList<JsonListColumn<T>> columns, bool idMayBeCode = false)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("organizationId", organizationId);

        var assignments = new List<string>();
        for (var i = 0; i < columns.Count; i++)
        {
            assignments.Add($"{CheckIdentifier(columns[i].Column)} = @p{i}");
            parameters.Add($"p{i}", System.Text.Json.JsonSerializer.Serialize(columns[i].Get(entity) ?? new List<string>()));
        }

        var idFilter = idMayBeCode ? "(id::VARCHAR = @id OR LOWER(code) = LOWER(@id))" : "id::VARCHAR = @id";
        var tenantFilter = organizationId.HasValue ? " AND organization_id = @organizationId" : string.Empty;
        await ExecuteAsync(
            $"UPDATE {CheckIdentifier(table)} SET {string.Join(", ", assignments)} WHERE {idFilter}{tenantFilter}",
            parameters);
    }

    /// <summary>Loads the JSON list columns for <paramref name="entities"/> in one query and assigns them.</summary>
    protected async Task LoadJsonListsAsync<T>(string table, IReadOnlyCollection<T> entities, Func<T, string> idOf, IReadOnlyList<JsonListColumn<T>> columns)
    {
        if (entities.Count == 0) return;

        var columnList = string.Join(", ", columns.Select(c => CheckIdentifier(c.Column)));
        var rows = await QueryListAsync<dynamic>(
            $"SELECT id::VARCHAR AS row_id, {columnList} FROM {CheckIdentifier(table)} WHERE id::VARCHAR = ANY(@ids)",
            new { ids = entities.Select(idOf).Distinct().ToArray() });

        var byId = rows.ToDictionary(r => (string)r.row_id, r => (IDictionary<string, object?>)r);
        foreach (var entity in entities)
        {
            if (!byId.TryGetValue(idOf(entity), out var row)) continue;
            foreach (var column in columns)
                column.Set(entity, ParseJsonList(row[column.Column] as string));
        }
    }

    /// <summary>Parses a JSON string array; null, empty or malformed values yield an empty list.</summary>
    protected static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (System.Text.Json.JsonException)
        {
            return new List<string>();
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
