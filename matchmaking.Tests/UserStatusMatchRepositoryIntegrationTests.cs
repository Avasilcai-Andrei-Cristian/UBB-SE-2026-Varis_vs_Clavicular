using System;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
namespace matchmaking.Tests;
[Collection("SqlIntegration")]
public sealed class UserStatusMatchRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;
    public UserStatusMatchRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }
    [Fact]
    public void GetByUserId_returns_matches_for_user()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        repository.Add(new Match { UserId = 1, JobId = 10, Status = MatchStatus.Applied, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty });
        repository.Add(new Match { UserId = 2, JobId = 11, Status = MatchStatus.Applied, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty });
        var statusRepository = new UserStatusMatchRepository(database.ConnectionString);
        var matches = statusRepository.GetByUserId(1);
        matches.Should().ContainSingle();
        matches[0].UserId.Should().Be(1);
    }
    [Fact]
    public void GetRejectedByUserId_filters_to_rejected_status()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        repository.Add(new Match { UserId = 3, JobId = 12, Status = MatchStatus.Rejected, Timestamp = DateTime.UtcNow, FeedbackMessage = "No" });
        repository.Add(new Match { UserId = 3, JobId = 13, Status = MatchStatus.Accepted, Timestamp = DateTime.UtcNow, FeedbackMessage = "Ok" });
        var statusRepository = new UserStatusMatchRepository(database.ConnectionString);
        var matches = statusRepository.GetRejectedByUserId(3);
        matches.Should().ContainSingle();
        matches[0].Status.Should().Be(MatchStatus.Rejected);
    }
    [Fact]
    public void Status_mapping_defaults_to_applied_for_unknown_value()
    {
        var connectionString = database.ConnectionString;
        var matchRepository = new SqlMatchRepository(connectionString);
        var match = new Match { UserId = 4, JobId = 20, Status = MatchStatus.Accepted, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty };
        matchRepository.Add(match);
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        connection.Open();
        using var command = new Microsoft.Data.SqlClient.SqlCommand("UPDATE Matches SET Status = 'Advanced' WHERE MatchID = @MatchId", connection);
        command.Parameters.AddWithValue("@MatchId", match.MatchId);
        command.ExecuteNonQuery();
        var statusRepository = new UserStatusMatchRepository(connectionString);
        var matches = statusRepository.GetByUserId(4);
        matches.Should().ContainSingle();
        matches[0].Status.Should().Be(MatchStatus.Applied);
    }
    [Fact]
    public void Feedback_is_empty_when_db_value_is_null()
    {
        var repository = new SqlMatchRepository(database.ConnectionString);
        var match = new Match { UserId = 6, JobId = 30, Status = MatchStatus.Applied, Timestamp = DateTime.UtcNow, FeedbackMessage = string.Empty };
        repository.Add(match);
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(database.ConnectionString);
        connection.Open();
        using var command = new Microsoft.Data.SqlClient.SqlCommand("UPDATE Matches SET Feedback = NULL WHERE MatchID = @MatchId", connection);
        command.Parameters.AddWithValue("@MatchId", match.MatchId);
        command.ExecuteNonQuery();
        var statusRepository = new UserStatusMatchRepository(database.ConnectionString);
        var matches = statusRepository.GetByUserId(6);
        matches.Should().ContainSingle();
        matches[0].FeedbackMessage.Should().BeEmpty();
    }
}
