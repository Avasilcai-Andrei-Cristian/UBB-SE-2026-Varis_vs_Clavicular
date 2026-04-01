using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Repositories;

public class SqlInteractionRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public Interaction? GetById(int interactionId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT InteractionId, DeveloperId, PostId, Type FROM Interaction WHERE InteractionId = @InteractionId",
            connection);
        command.Parameters.AddWithValue("@InteractionId", interactionId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IReadOnlyList<Interaction> GetAll()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT InteractionId, DeveloperId, PostId, Type FROM Interaction",
            connection);
        using var reader = command.ExecuteReader();

        var result = new List<Interaction>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public IReadOnlyList<Interaction> GetByDeveloperId(int developerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT InteractionId, DeveloperId, PostId, Type FROM Interaction WHERE DeveloperId = @DeveloperId",
            connection);
        command.Parameters.AddWithValue("@DeveloperId", developerId);

        using var reader = command.ExecuteReader();
        var result = new List<Interaction>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public IReadOnlyList<Interaction> GetByPostId(int postId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT InteractionId, DeveloperId, PostId, Type FROM Interaction WHERE PostId = @PostId",
            connection);
        command.Parameters.AddWithValue("@PostId", postId);

        using var reader = command.ExecuteReader();
        var result = new List<Interaction>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Interaction interaction)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Interaction (DeveloperId, PostId, Type) VALUES (@DeveloperId, @PostId, @Type)",
            connection);
        command.Parameters.AddWithValue("@DeveloperId", interaction.DeveloperId);
        command.Parameters.AddWithValue("@PostId", interaction.PostId);
        command.Parameters.AddWithValue("@Type", (int)interaction.Type);
        command.ExecuteNonQuery();
    }

    public void Update(Interaction interaction)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Interaction SET DeveloperId = @DeveloperId, PostId = @PostId, Type = @Type WHERE InteractionId = @InteractionId",
            connection);
        command.Parameters.AddWithValue("@InteractionId", interaction.InteractionId);
        command.Parameters.AddWithValue("@DeveloperId", interaction.DeveloperId);
        command.Parameters.AddWithValue("@PostId", interaction.PostId);
        command.Parameters.AddWithValue("@Type", (int)interaction.Type);
        command.ExecuteNonQuery();
    }

    public void Remove(int interactionId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "DELETE FROM Interaction WHERE InteractionId = @InteractionId",
            connection);
        command.Parameters.AddWithValue("@InteractionId", interactionId);
        command.ExecuteNonQuery();
    }

    private static Interaction Map(SqlDataReader reader)
    {
        var rawType = reader.GetValue(3);
        var interactionType = rawType switch
        {
            bool isLike => isLike ? InteractionType.Like : InteractionType.Dislike,
            byte numericType => Enum.IsDefined(typeof(InteractionType), (int)numericType)
                ? (InteractionType)numericType
                : InteractionType.Dislike,
            short numericType => Enum.IsDefined(typeof(InteractionType), (int)numericType)
                ? (InteractionType)numericType
                : InteractionType.Dislike,
            int numericType => Enum.IsDefined(typeof(InteractionType), numericType)
                ? (InteractionType)numericType
                : InteractionType.Dislike,
            long numericType when numericType is >= int.MinValue and <= int.MaxValue
                && Enum.IsDefined(typeof(InteractionType), (int)numericType) => (InteractionType)numericType,
            _ => InteractionType.Dislike
        };

        return new Interaction
        {
            InteractionId = reader.GetInt32(0),
            DeveloperId = reader.GetInt32(1),
            PostId = reader.GetInt32(2),
            Type = interactionType
        };
    }
}
