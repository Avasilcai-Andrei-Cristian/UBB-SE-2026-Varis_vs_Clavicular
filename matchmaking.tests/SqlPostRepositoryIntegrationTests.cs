namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlPostRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlPostRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void SelectMapInsertUpdateDeleteAndFiltering_WhenPostsExist_ShouldRoundTripAgainstDb()
    {
        var developerId = InsertDeveloper("Dev");
        var otherDeveloperId = InsertDeveloper("Other Dev");
        var repository = new SqlPostRepository(database.ConnectionString);

        repository.Add(new Post { DeveloperId = developerId, ParameterType = PostParameterType.MitigationFactor, Value = "3" });
        repository.Add(new Post { DeveloperId = otherDeveloperId, ParameterType = PostParameterType.RelevantKeyword, Value = "sql" });

        var allPosts = repository.GetAll();
        allPosts.Should().HaveCount(2);
        allPosts.Should().Contain(item => item.ParameterType == PostParameterType.MitigationFactor && item.Value == "3");

        var first = allPosts.Single(item => item.DeveloperId == developerId);
        first.Value = "8";
        repository.Update(first);

        var updated = repository.GetById(first.PostId);
        updated.Should().NotBeNull();
        updated!.Value.Should().Be("8");
        repository.GetByDeveloperId(developerId).Should().ContainSingle();

        repository.Remove(first.PostId);
        repository.GetById(first.PostId).Should().BeNull();
    }

    private int InsertDeveloper(string name)
    {
        return database.ExecuteScalar<int>(
            "INSERT INTO Developer (Name, Password) VALUES (@Name, @Password); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            parameters =>
            {
                parameters.AddWithValue("@Name", name);
                parameters.AddWithValue("@Password", "pwd");
            });
    }
}
