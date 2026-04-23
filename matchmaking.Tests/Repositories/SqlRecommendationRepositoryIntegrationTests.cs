namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlRecommendationRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlRecommendationRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void SelectMapUpdateAndDeletePaths_WhenRecommendationExists_ShouldRoundTripAgainstDb()
    {
        var recommendationId = InsertRecommendation(5, 8, new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc));
        var repository = new SqlRecommendationRepository(database.ConnectionString);

        var item = repository.GetById(recommendationId);
        item.Should().NotBeNull();
        item!.UserId.Should().Be(5);

        item.JobId = 11;
        item.Timestamp = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc);
        repository.Update(item);

        var updated = repository.GetById(recommendationId);
        updated.Should().NotBeNull();
        updated!.JobId.Should().Be(11);

        repository.Remove(recommendationId);
        repository.GetById(recommendationId).Should().BeNull();
    }

    [Fact]
    public void InsertAndTimestampQueryPaths_WhenMultipleRowsExist_ShouldReturnLatestByTimestamp()
    {
        var repository = new SqlRecommendationRepository(database.ConnectionString);
        repository.InsertReturningId(new Recommendation
        {
            UserId = 10,
            JobId = 20,
            Timestamp = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc)
        });
        var newestId = repository.InsertReturningId(new Recommendation
        {
            UserId = 10,
            JobId = 20,
            Timestamp = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
        });

        var latest = repository.GetLatestByUserIdAndJobId(10, 20);
        latest.Should().NotBeNull();
        latest!.RecommendationId.Should().Be(newestId);
        latest.Timestamp.Should().Be(new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
    }

    private int InsertRecommendation(int userId, int jobId, DateTime timestamp)
    {
        return database.ExecuteScalar<int>(
            "INSERT INTO Recommendation (UserID, JobID, Timestamp) VALUES (@UserId, @JobId, @Timestamp); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            parameters =>
            {
                parameters.AddWithValue("@UserId", userId);
                parameters.AddWithValue("@JobId", jobId);
                parameters.AddWithValue("@Timestamp", timestamp);
            });
    }
}
