using ISOFlow.Infrastructure.Data;
using Npgsql;

namespace ISOFlow.Tests.Infrastructure;

/// <summary>
/// Integration-test database settings. Tests that need PostgreSQL read their connection string from the
/// <c>ISOFLOW_TEST_DB</c> environment variable and are skipped (not failed) when it is not set.
/// </summary>
public static class TestDatabase
{
    public const string EnvironmentVariable = "ISOFLOW_TEST_DB";

    public static string? ConnectionString =>
        Environment.GetEnvironmentVariable(EnvironmentVariable) is { Length: > 0 } value ? value : null;

    public static bool IsConfigured => ConnectionString != null;

    public static string SkipReason =>
        $"Integration test: set {EnvironmentVariable} to a dedicated PostgreSQL test database connection string " +
        "(database name must contain 'test'; it is wiped and re-seeded on every run).";

    public static IDbConnectionFactory CreateFactory() =>
        new DbConnectionFactory(ConnectionString ?? throw new InvalidOperationException(SkipReason));
}

/// <summary>A <see cref="FactAttribute"/> that is skipped when no test database is configured.</summary>
public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute()
    {
        if (!TestDatabase.IsConfigured)
            Skip = TestDatabase.SkipReason;
    }
}

/// <summary>
/// Rebuilds the test database from the schema scripts (01–04) once per test run so every run starts from
/// the same seed. Refuses to touch any database whose name does not contain "test".
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private static readonly string[] SchemaScripts =
    {
        "01_Tables_And_Constraints.sql",
        "02_Stored_Procedures.sql",
        "03_Functions.sql",
        "04_Seed_Data.sql"
    };

    public async Task InitializeAsync()
    {
        if (!TestDatabase.IsConfigured) return;

        var builder = new NpgsqlConnectionStringBuilder(TestDatabase.ConnectionString);
        if (string.IsNullOrEmpty(builder.Database) || !builder.Database.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to reset database '{builder.Database}': the {TestDatabase.EnvironmentVariable} database name must contain 'test'.");
        }

        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "DatabaseSchema");
        await using var connection = new NpgsqlConnection(TestDatabase.ConnectionString);
        await connection.OpenAsync();

        foreach (var script in SchemaScripts)
        {
            var sql = await File.ReadAllTextAsync(Path.Combine(scriptDirectory, script));
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// All database tests share one fixture and run serially, because they read and write the same seeded data.
/// Usage: <c>[Collection(DatabaseCollection.Name)]</c> on the test class.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}
