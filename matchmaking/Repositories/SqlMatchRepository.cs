using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Repositories;

public class SqlMatchRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public Match? GetById(int matchId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MatchID, UserID, JobID, Status, Timestamp, Feedback FROM [Matches] WHERE MatchID = @MatchId",
            connection);
        command.Parameters.AddWithValue("@MatchId", matchId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IReadOnlyList<Match> GetAll()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MatchID, UserID, JobID, Status, Timestamp, Feedback FROM [Matches]",
            connection);
        using var reader = command.ExecuteReader();

        var result = new List<Match>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Match match)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO [Matches] (MatchID, UserID, JobID, Status, Timestamp, Feedback) VALUES (@MatchId, @UserId, @JobId, @Status, @Timestamp, @Feedback)",
            connection);
        command.Parameters.AddWithValue("@MatchId", match.MatchId);
        command.Parameters.AddWithValue("@UserId", match.UserId);
        command.Parameters.AddWithValue("@JobId", match.JobId);
        command.Parameters.AddWithValue("@Status", ToDbStatus(match.Status));
        command.Parameters.AddWithValue("@Timestamp", match.Timestamp);
        command.Parameters.AddWithValue("@Feedback", string.IsNullOrWhiteSpace(match.FeedbackMessage)
            ? DBNull.Value
            : match.FeedbackMessage);
        command.ExecuteNonQuery();
    }

    public void Update(Match match)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE [Matches] SET UserID = @UserId, JobID = @JobId, Status = @Status, Timestamp = @Timestamp, Feedback = @Feedback WHERE MatchID = @MatchId",
            connection);
        command.Parameters.AddWithValue("@MatchId", match.MatchId);
        command.Parameters.AddWithValue("@UserId", match.UserId);
        command.Parameters.AddWithValue("@JobId", match.JobId);
        command.Parameters.AddWithValue("@Status", ToDbStatus(match.Status));
        command.Parameters.AddWithValue("@Timestamp", match.Timestamp);
        command.Parameters.AddWithValue("@Feedback", string.IsNullOrWhiteSpace(match.FeedbackMessage)
            ? DBNull.Value
            : match.FeedbackMessage);
        command.ExecuteNonQuery();
    }

    public void Remove(int matchId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "DELETE FROM [Matches] WHERE MatchID = @MatchId",
            connection);
        command.Parameters.AddWithValue("@MatchId", matchId);
        command.ExecuteNonQuery();
    }

    private static Match Map(SqlDataReader reader)
    {
        var rawStatus = reader.GetString(3);

        return new Match
        {
            MatchId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            JobId = reader.GetInt32(2),
            Status = FromDbStatus(rawStatus),
            Timestamp = reader.GetDateTime(4),
            FeedbackMessage = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
        };
    }

    // TODO: Schema conflict — main uses [Matches]/Feedback/nvarchar Status.
    // Consensus across other branches is Match/FeedbackMessage/int Status.
    // Team needs to align on a single schema.
    private static MatchStatus FromDbStatus(string rawStatus)
    {
        if (rawStatus.Equals("accepted", StringComparison.OrdinalIgnoreCase))
        {
            return MatchStatus.Accepted;
        }

        if (rawStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase))
        {
            return MatchStatus.Rejected;
        }

        if (rawStatus.Equals("advanced", StringComparison.OrdinalIgnoreCase))
        {
            return MatchStatus.Advanced;
        }

        return MatchStatus.Applied;
    }

    private static string ToDbStatus(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => "Accepted",
            MatchStatus.Rejected => "Rejected",
            MatchStatus.Advanced => "Advanced",
            _ => "Pending"
        };
    }
}
