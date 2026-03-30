using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Repositories;

public class SqlPostRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public Post? GetById(int postId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT PostId, DeveloperId, Parameter, Value FROM Post WHERE PostId = @PostId",
            connection);
        command.Parameters.AddWithValue("@PostId", postId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IReadOnlyList<Post> GetAll()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT PostId, DeveloperId, Parameter, Value FROM Post",
            connection);
        using var reader = command.ExecuteReader();

        var result = new List<Post>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public IReadOnlyList<Post> GetByDeveloperId(int developerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT PostId, DeveloperId, Parameter, Value FROM Post WHERE DeveloperId = @DeveloperId",
            connection);
        command.Parameters.AddWithValue("@DeveloperId", developerId);

        using var reader = command.ExecuteReader();
        var result = new List<Post>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Post post)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Post (PostId, DeveloperId, Parameter, Value) VALUES (@PostId, @DeveloperId, @Parameter, @Value)",
            connection);
        command.Parameters.AddWithValue("@PostId", post.PostId);
        command.Parameters.AddWithValue("@DeveloperId", post.DeveloperId);
        command.Parameters.AddWithValue("@Parameter", PostParameterTypeMapper.ToStorageValue(post.ParameterType));
        command.Parameters.AddWithValue("@Value", post.Value);
        command.ExecuteNonQuery();
    }

    public void Update(Post post)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Post SET DeveloperId = @DeveloperId, Parameter = @Parameter, Value = @Value WHERE PostId = @PostId",
            connection);
        command.Parameters.AddWithValue("@PostId", post.PostId);
        command.Parameters.AddWithValue("@DeveloperId", post.DeveloperId);
        command.Parameters.AddWithValue("@Parameter", PostParameterTypeMapper.ToStorageValue(post.ParameterType));
        command.Parameters.AddWithValue("@Value", post.Value);
        command.ExecuteNonQuery();
    }

    public void Remove(int postId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "DELETE FROM Post WHERE PostId = @PostId",
            connection);
        command.Parameters.AddWithValue("@PostId", postId);
        command.ExecuteNonQuery();
    }

    private static Post Map(SqlDataReader reader)
    {
        return new Post
        {
            PostId = reader.GetInt32(0),
            DeveloperId = reader.GetInt32(1),
            ParameterType = PostParameterTypeMapper.FromStorageValue(reader.GetString(2)),
            Value = reader.GetString(3)
        };
    }
}
