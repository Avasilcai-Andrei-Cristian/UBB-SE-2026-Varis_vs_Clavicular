using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Repositories;

public class UserStatusMatchRepository : SqlRepositoryBase, IUserStatusMatchRepository
{
    public UserStatusMatchRepository(string connectionString)
        : base(connectionString)
    {
    }

    public IReadOnlyList<Match> GetByUserId(int userId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MatchID, UserID, JobID, Status, Timestamp, Feedback FROM Matches WHERE UserID = @UserId",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();
        var result = new List<Match>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public IReadOnlyList<Match> GetRejectedByUserId(int userId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MatchID, UserID, JobID, Status, Timestamp, Feedback FROM Matches WHERE UserID = @UserId AND Status = 'Rejected'",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();
        var result = new List<Match>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    private static Match Map(SqlDataReader reader)
    {
        var status = reader.GetString(3) switch
        {
            "Accepted" => MatchStatus.Accepted,
            "Rejected" => MatchStatus.Rejected,
            _ => MatchStatus.Applied
        };

        return new Match
        {
            MatchId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            JobId = reader.GetInt32(2),
            Status = status,
            Timestamp = reader.GetDateTime(4),
            FeedbackMessage = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
        };
    }
}
