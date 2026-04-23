namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlConnectionTestRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlConnectionTestRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void Ping_WhenConnectionIsValid_ReturnsOne()
    {
        var repository = new SqlConnectionTestRepository(database.ConnectionString);

        repository.Ping().Should().Be(1);
    }
}
