using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace matchmaking.Tests;

public sealed class SqlIntegrationTestDatabase : IAsyncLifetime
{
    private readonly string databaseName = $"matchmaking_tests_{Guid.NewGuid():N}";
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync().ConfigureAwait(false);
        await ApplySchemaAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await DropDatabaseAsync().ConfigureAwait(false);
    }

    public Task ResetAsync()
    {
        return ApplySchemaAsync();
    }

    private async Task CreateDatabaseAsync()
    {
        var masterConnectionString = BuildMasterConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var commandText = $"CREATE DATABASE [{databaseName}]";
        await using var command = new SqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);

        ConnectionString = BuildDatabaseConnectionString(databaseName);
    }

    private async Task ApplySchemaAsync()
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "matchmaking", "finalSQL.sql");
        var script = await File.ReadAllTextAsync(scriptPath).ConfigureAwait(false);

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        await using var command = new SqlCommand(script, connection);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private async Task DropDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        var masterConnectionString = BuildMasterConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var commandText = $@"
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{databaseName}')
BEGIN
    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{databaseName}];
END";

        await using var command = new SqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static string BuildMasterConnectionString()
    {
        return "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";
    }

    private static string BuildDatabaseConnectionString(string database)
    {
        return $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true;";
    }
}
