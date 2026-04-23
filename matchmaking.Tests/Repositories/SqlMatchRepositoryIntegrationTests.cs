namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlMatchRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlMatchRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void SelectMapInsertUpdateDeletePaths_WhenMatchRoundTrip_ShouldPersistAgainstDatabase()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var insertedId = repository.InsertReturningId(new Match
        {
            UserId = 15,
            JobId = 100,
            Status = MatchStatus.Applied,
            Timestamp = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            FeedbackMessage = string.Empty
        });

        var match = repository.GetById(insertedId);
        match.Should().NotBeNull();
        match!.Status.Should().Be(MatchStatus.Applied);

        match.Status = MatchStatus.Accepted;
        match.FeedbackMessage = "Great fit";
        repository.Update(match);

        var byUserAndJob = repository.GetByUserIdAndJobId(15, 100);
        byUserAndJob.Should().NotBeNull();
        byUserAndJob!.Status.Should().Be(MatchStatus.Accepted);
        byUserAndJob.FeedbackMessage.Should().Be("Great fit");

        repository.GetAll().Should().ContainSingle(item => item.MatchId == insertedId);

        repository.Remove(insertedId);
        repository.GetById(insertedId).Should().BeNull();
    }
}
