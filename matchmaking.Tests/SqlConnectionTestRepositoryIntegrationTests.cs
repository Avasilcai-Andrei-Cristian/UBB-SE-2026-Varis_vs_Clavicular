namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlConnectionTestRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlConnectionTestRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Ping_returns_one_when_connected_to_database()
    {
        var repository = new SqlConnectionTestRepository(database.ConnectionString);
        const int expectedPingResult = 1;
        var result = repository.Ping();
        result.Should().Be(expectedPingResult);
    }
}