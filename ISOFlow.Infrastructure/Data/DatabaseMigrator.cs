using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Helpers;
using Npgsql;

namespace ISOFlow.Infrastructure.Data;

/// <summary>Options for one <see cref="DatabaseMigrator.Run"/>.</summary>
public sealed class DatabaseMigrationOptions
{
    /// <summary>Create the database itself if it doesn't exist (development convenience).</summary>
    public bool CreateDatabaseIfMissing { get; init; }

    /// <summary>Apply the dev-only demo data (Seed scripts). Never enable in production.</summary>
    public bool SeedDemoData { get; init; }
}

/// <summary>Outcome of a migration run.</summary>
public sealed record DatabaseMigrationResult(bool Successful, IReadOnlyList<string> AppliedScripts, Exception? Error);

/// <summary>
/// Brings a PostgreSQL database up to date with the embedded scripts (see ISOFlow.Domain/DatabaseSchema), using DbUp:
/// <list type="number">
/// <item><b>Baseline</b> — a database created before DbUp was introduced (tables exist, no <c>schemaversions</c>
/// journal) has migration 0001 recorded as applied without running it.</item>
/// <item><b>Migrations</b> — numbered, run once each, recorded in <c>schemaversions</c>.</item>
/// <item><b>Programmability</b> — stored procedures and functions (<c>CREATE OR REPLACE</c>), re-applied on every run
/// so they always match the code.</item>
/// <item><b>Seed</b> — dev-only demo data, run once, only when <see cref="DatabaseMigrationOptions.SeedDemoData"/> is set.</item>
/// </list>
/// Each script runs in its own transaction. Variable substitution is disabled so <c>$...$</c> in SQL (dollar quoting,
/// password hashes) is left untouched.
/// </summary>
public static class DatabaseMigrator
{
    private const string ResourcePrefix = "ISOFlow.Schema.";
    private const string InitialMigration = ResourcePrefix + "Migrations.0001_Initial_Schema.sql";

    private static readonly Assembly ScriptAssembly = typeof(DatabaseMigrator).Assembly;

    public static DatabaseMigrationResult Run(string connectionString, DatabaseMigrationOptions options)
    {
        var applied = new List<string>();
        try
        {
            if (options.CreateDatabaseIfMissing)
                EnsureDatabase.For.PostgresqlDatabase(connectionString, new NoOpUpgradeLog());

            BaselineLegacyDatabase(connectionString, applied);

            if (!RunStage(Engine(connectionString, "Migrations."), applied, out var error)) return new(false, applied, error);
            if (!RunStage(Engine(connectionString, "Programmability.", alwaysRun: true), applied, out error)) return new(false, applied, error);
            if (options.SeedDemoData && !RunStage(Engine(connectionString, "Seed."), applied, out error)) return new(false, applied, error);

            return new(true, applied, null);
        }
        catch (Exception ex)
        {
            return new(false, applied, ex);
        }
    }

    /// <summary>Names of all embedded scripts, in execution order per stage (for diagnostics and tests).</summary>
    public static IReadOnlyList<string> EmbeddedScriptNames() =>
        ScriptAssembly.GetManifestResourceNames().Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal)).Order().ToList();

    private static bool RunStage(UpgradeEngine engine, List<string> applied, out Exception? error)
    {
        var result = engine.PerformUpgrade();
        applied.AddRange(result.Scripts.Select(s => s.Name));
        error = result.Successful ? null : result.Error;
        return result.Successful;
    }

    /// <summary>Records 0001 as applied on databases whose schema predates the migration journal.</summary>
    private static void BaselineLegacyDatabase(string connectionString, List<string> applied)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand(
            "SELECT to_regclass('public.users') IS NOT NULL AND to_regclass('public.schemaversions') IS NULL", connection);
        if (command.ExecuteScalar() is not true) return;

        Engine(connectionString, "Migrations.", exactScript: InitialMigration).MarkAsExecuted();
        applied.Add(InitialMigration + " (baselined, not executed)");
    }

    private static UpgradeEngine Engine(string connectionString, string folder, bool alwaysRun = false, string? exactScript = null)
    {
        var builder = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(ScriptAssembly, name =>
                exactScript != null ? name == exactScript : name.StartsWith(ResourcePrefix + folder, StringComparison.Ordinal))
            .WithTransactionPerScript()
            .WithVariablesDisabled()
            .LogToNowhere();

        if (alwaysRun)
            builder = builder.JournalTo(new NullJournal());

        return builder.Build();
    }
}
