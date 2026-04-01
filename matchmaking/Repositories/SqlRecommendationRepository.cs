using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class SqlRecommendationRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public Recommendation? GetById(int recommendationId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT RecommendationId, UserId, JobId, Timestamp FROM Recommendation WHERE RecommendationId = @RecommendationId",
            connection);
        command.Parameters.AddWithValue("@RecommendationId", recommendationId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IReadOnlyList<Recommendation> GetAll()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT RecommendationId, UserId, JobId, Timestamp FROM Recommendation",
            connection);
        using var reader = command.ExecuteReader();

        var result = new List<Recommendation>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Recommendation recommendation)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Recommendation (RecommendationId, UserId, JobId, Timestamp) VALUES (@RecommendationId, @UserId, @JobId, @Timestamp)",
            connection);
        command.Parameters.AddWithValue("@RecommendationId", recommendation.RecommendationId);
        command.Parameters.AddWithValue("@UserId", recommendation.UserId);
        command.Parameters.AddWithValue("@JobId", recommendation.JobId);
        command.Parameters.AddWithValue("@Timestamp", recommendation.Timestamp);
        command.ExecuteNonQuery();
    }

    public void Update(Recommendation recommendation)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Recommendation SET UserId = @UserId, JobId = @JobId, Timestamp = @Timestamp WHERE RecommendationId = @RecommendationId",
            connection);
        command.Parameters.AddWithValue("@RecommendationId", recommendation.RecommendationId);
        command.Parameters.AddWithValue("@UserId", recommendation.UserId);
        command.Parameters.AddWithValue("@JobId", recommendation.JobId);
        command.Parameters.AddWithValue("@Timestamp", recommendation.Timestamp);
        command.ExecuteNonQuery();
    }

    public void Remove(int recommendationId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "DELETE FROM Recommendation WHERE RecommendationId = @RecommendationId",
            connection);
        command.Parameters.AddWithValue("@RecommendationId", recommendationId);
        command.ExecuteNonQuery();
    }

    public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            """
            SELECT TOP (1) RecommendationId, UserId, JobId, Timestamp
            FROM Recommendation
            WHERE UserId = @UserId AND JobId = @JobId
            ORDER BY Timestamp DESC
            """,
            connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@JobId", jobId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public int InsertReturningId(Recommendation recommendation)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            """
            INSERT INTO Recommendation (UserId, JobId, Timestamp)
            OUTPUT INSERTED.RecommendationId
            VALUES (@UserId, @JobId, @Timestamp)
            """,
            connection);
        command.Parameters.AddWithValue("@UserId", recommendation.UserId);
        command.Parameters.AddWithValue("@JobId", recommendation.JobId);
        command.Parameters.AddWithValue("@Timestamp", recommendation.Timestamp);
        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    private static Recommendation Map(SqlDataReader reader)
    {
        return new Recommendation
        {
            RecommendationId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            JobId = reader.GetInt32(2),
            Timestamp = reader.GetDateTime(3)
        };
    }
}
