namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlDeveloperRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlDeveloperRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void SelectMapInsertUpdateDeletePaths_WhenDeveloperRoundTrip_ShouldPersistAgainstDatabase()
    {
        var repository = new SqlDeveloperRepository(database.ConnectionString);
        repository.Add(new Developer { Name = "Dev One", Password = "pass-1" });
        var insertedId = database.ExecuteScalar<int>("SELECT TOP 1 DeveloperID FROM Developer ORDER BY DeveloperID DESC");

        var inserted = repository.GetById(insertedId);
        inserted.Should().NotBeNull();
        inserted!.Name.Should().Be("Dev One");
        inserted.Password.Should().Be("pass-1");

        inserted.Name = "Dev Updated";
        inserted.Password = "pass-2";
        repository.Update(inserted);

        var updated = repository.GetById(insertedId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Dev Updated");
        repository.GetAll().Should().ContainSingle(item => item.DeveloperId == insertedId);

        repository.Remove(insertedId);
        repository.GetById(insertedId).Should().BeNull();
    }
}
