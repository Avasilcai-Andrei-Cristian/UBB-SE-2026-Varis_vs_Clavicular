namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlInteractionRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlInteractionRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void SelectMapInsertUpdateDeleteAndFiltering_WhenInteractionsExist_ShouldRoundTripAgainstDb()
    {
        var developerId = InsertDeveloper("Dev");
        var otherDeveloperId = InsertDeveloper("Other");
        var postId = InsertPost(developerId, "mitigation factor", "4");
        var otherPostId = InsertPost(otherDeveloperId, "relevant keyword", "azure");
        var repository = new SqlInteractionRepository(database.ConnectionString);

        repository.Add(new Interaction { DeveloperId = developerId, PostId = postId, Type = InteractionType.Like });
        repository.Add(new Interaction { DeveloperId = otherDeveloperId, PostId = otherPostId, Type = InteractionType.Dislike });

        repository.GetByDeveloperId(developerId).Should().ContainSingle().Which.Type.Should().Be(InteractionType.Like);
        repository.GetByPostId(otherPostId).Should().ContainSingle().Which.Type.Should().Be(InteractionType.Dislike);

        var existing = repository.GetByDeveloperIdAndPostId(developerId, postId);
        existing.Should().NotBeNull();

        existing!.Type = InteractionType.Dislike;
        repository.Update(existing);

        var updated = repository.GetById(existing.InteractionId);
        updated.Should().NotBeNull();
        updated!.Type.Should().Be(InteractionType.Dislike);

        repository.Remove(existing.InteractionId);
        repository.GetById(existing.InteractionId).Should().BeNull();
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

    private int InsertPost(int developerId, string parameter, string value)
    {
        return database.ExecuteScalar<int>(
            "INSERT INTO Post (DeveloperID, Parameter, Value) VALUES (@DeveloperId, @Parameter, @Value); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            parameters =>
            {
                parameters.AddWithValue("@DeveloperId", developerId);
                parameters.AddWithValue("@Parameter", parameter);
                parameters.AddWithValue("@Value", value);
            });
    }
}
