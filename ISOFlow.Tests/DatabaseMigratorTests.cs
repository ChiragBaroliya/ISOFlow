using Dapper;
using ISOFlow.Application.Helpers;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Tests.Infrastructure;
using Npgsql;

namespace ISOFlow.Tests;

[Collection(DatabaseCollection.Name)]
public class DatabaseMigratorTests
{
    [Fact]
    public void EmbeddedScripts_AreAllPresent_InExecutionOrder()
    {
        var names = DatabaseMigrator.EmbeddedScriptNames();

        Assert.Equal(new[]
        {
            "ISOFlow.Schema.Migrations.0001_Initial_Schema.sql",
            "ISOFlow.Schema.Migrations.0002_Security_And_Snapshots.sql",
            "ISOFlow.Schema.Programmability.0001_Stored_Procedures.sql",
            "ISOFlow.Schema.Programmability.0002_Functions.sql",
            "ISOFlow.Schema.Seed.0001_Demo_Data.sql"
        }, names);
    }

    [DatabaseFact]
    public void Rerun_OnUpToDateDatabase_OnlyReappliesProgrammability()
    {
        // The fixture already migrated and seeded this database.
        var result = DatabaseMigrator.Run(TestDatabase.ConnectionString!, new DatabaseMigrationOptions { SeedDemoData = true });

        Assert.True(result.Successful, result.Error?.ToString());
        Assert.All(result.AppliedScripts, s => Assert.StartsWith("ISOFlow.Schema.Programmability.", s));
        Assert.Equal(2, result.AppliedScripts.Count);
    }

    [DatabaseFact]
    public async Task LegacyDatabase_WithoutJournal_IsBaselinedAndUpgraded_WithoutDataLoss()
    {
        await using var conn = new NpgsqlConnection(TestDatabase.ConnectionString);
        await conn.OpenAsync();

        // Simulate a database created by the old hand-run scripts: no migration journal,
        // and a user still holding the old plain-text default password.
        await conn.ExecuteAsync("DROP TABLE schemaversions;");
        var legacyUserId = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO users (organization_id, name, email, password_hash, system_role, role, department, location)
              VALUES (1, 'Legacy User', 'legacy.user@test.local', 'Test@123', 2, 'Analyst', 'QA', 'Remote') RETURNING id;");
        var usersBefore = await conn.ExecuteScalarAsync<long>("SELECT count(*) FROM users;");

        var result = DatabaseMigrator.Run(TestDatabase.ConnectionString!, new DatabaseMigrationOptions { SeedDemoData = false });

        Assert.True(result.Successful, result.Error?.ToString());
        Assert.Contains(result.AppliedScripts, s => s.Contains("0001_Initial_Schema") && s.Contains("baselined"));
        Assert.Contains("ISOFlow.Schema.Migrations.0002_Security_And_Snapshots.sql", result.AppliedScripts);

        // No data lost, and 0002 hashed the legacy plain-text password (same password still works).
        Assert.Equal(usersBefore, await conn.ExecuteScalarAsync<long>("SELECT count(*) FROM users;"));
        var hash = await conn.ExecuteScalarAsync<string>("SELECT password_hash FROM users WHERE id = @legacyUserId;", new { legacyUserId });
        Assert.True(PasswordSecurityHelper.VerifyPassword("Test@123", hash!));

        // Journal now records both migrations, so a further run applies nothing new.
        var journal = (await conn.QueryAsync<string>("SELECT scriptname FROM schemaversions ORDER BY scriptname;")).ToList();
        Assert.Contains("ISOFlow.Schema.Migrations.0001_Initial_Schema.sql", journal);
        Assert.Contains("ISOFlow.Schema.Migrations.0002_Security_And_Snapshots.sql", journal);

        await conn.ExecuteAsync("DELETE FROM users WHERE id = @legacyUserId;", new { legacyUserId });
    }

    [DatabaseFact]
    public async Task FreshDatabase_IsBuiltFromScratch_WithSeedOnlyWhenRequested()
    {
        await DatabaseFixture.ResetSchemaAsync();

        var withoutSeed = DatabaseMigrator.Run(TestDatabase.ConnectionString!, new DatabaseMigrationOptions { SeedDemoData = false });
        Assert.True(withoutSeed.Successful, withoutSeed.Error?.ToString());
        Assert.DoesNotContain(withoutSeed.AppliedScripts, s => s.Contains(".Seed."));

        await using (var conn = new NpgsqlConnection(TestDatabase.ConnectionString))
        {
            Assert.Equal(0, await conn.ExecuteScalarAsync<long>("SELECT count(*) FROM users;"));
        }

        // Restore the seeded state the other database tests rely on.
        var withSeed = DatabaseMigrator.Run(TestDatabase.ConnectionString!, new DatabaseMigrationOptions { SeedDemoData = true });
        Assert.True(withSeed.Successful, withSeed.Error?.ToString());
        Assert.Contains("ISOFlow.Schema.Seed.0001_Demo_Data.sql", withSeed.AppliedScripts);
    }
}
