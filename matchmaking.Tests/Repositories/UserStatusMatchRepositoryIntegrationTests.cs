namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class UserStatusMatchRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public UserStatusMatchRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void QueryFilteringAndMapping_WhenUserHasMixedStatuses_ShouldReturnExpectedRows()
    {
        InsertMatch(200, 42, "Accepted", new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc));
        InsertMatch(201, 42, "Rejected", new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));
        InsertMatch(202, 42, "Advanced", new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc));
        InsertMatch(203, 99, "Rejected", new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));

        var repository = new UserStatusMatchRepository(database.ConnectionString);

        var allForUser = repository.GetByUserId(42);
        allForUser.Should().HaveCount(3);
        allForUser.Single(item => item.JobId == 200).Status.Should().Be(MatchStatus.Accepted);
        allForUser.Single(item => item.JobId == 202).Status.Should().Be(MatchStatus.Applied);

        var rejectedForUser = repository.GetRejectedByUserId(42);
        rejectedForUser.Should().ContainSingle();
        rejectedForUser[0].Status.Should().Be(MatchStatus.Rejected);
        rejectedForUser[0].JobId.Should().Be(201);
    }

    private void InsertMatch(int jobId, int userId, string status, DateTime timestamp)
    {
        database.ExecuteNonQuery(
            "INSERT INTO Matches (UserID, JobID, Status, Timestamp, Feedback) VALUES (@UserId, @JobId, @Status, @Timestamp, @Feedback)",
            parameters =>
            {
                parameters.AddWithValue("@UserId", userId);
                parameters.AddWithValue("@JobId", jobId);
                parameters.AddWithValue("@Status", status);
                parameters.AddWithValue("@Timestamp", timestamp);
                parameters.AddWithValue("@Feedback", "seed");
            });
    }
}
