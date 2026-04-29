using System;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
namespace matchmaking.Tests;
[Collection("SqlIntegration")]
public sealed class SqlMatchRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;
    public SqlMatchRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }
    [Fact]
    public void InsertReturningId_round_trips_status_and_feedback()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var match = new Match
        {
            UserId = 1,
            JobId = 2,
            Status = MatchStatus.Accepted,
            Timestamp = new DateTime(2026, 1, 1),
            FeedbackMessage = "Nice"
        };
        var id = repository.InsertReturningId(match);
        var loaded = repository.GetById(id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(MatchStatus.Accepted);
        loaded.FeedbackMessage.Should().Be("Nice");
    }
    [Fact]
    public void GetByUserIdAndJobId_returns_match()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var match = new Match { UserId = 3, JobId = 4, Status = MatchStatus.Applied, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty };
        repository.Add(match);
        var loaded = repository.GetByUserIdAndJobId(3, 4);
        loaded.Should().NotBeNull();
        loaded!.MatchId.Should().Be(match.MatchId);
    }
    [Fact]
    public void Update_persists_status_changes()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var match = new Match { UserId = 5, JobId = 6, Status = MatchStatus.Applied, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty };
        repository.Add(match);
        match.Status = MatchStatus.Rejected;
        match.FeedbackMessage = "No";
        repository.Update(match);
        var loaded = repository.GetById(match.MatchId);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(MatchStatus.Rejected);
        loaded.FeedbackMessage.Should().Be("No");
    }
    [Fact]
    public void GetAll_returns_inserted_matches()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        repository.Add(new Match { UserId = 7, JobId = 8, Status = MatchStatus.Applied, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty });
        repository.Add(new Match { UserId = 9, JobId = 10, Status = MatchStatus.Accepted, Timestamp = DateTime.UtcNow, FeedbackMessage = "Ok" });
        var matches = repository.GetAll();
        matches.Should().HaveCount(2);
        matches.Select(item => item.UserId).Should().Contain(new[] { 7, 9 });
    }
}
