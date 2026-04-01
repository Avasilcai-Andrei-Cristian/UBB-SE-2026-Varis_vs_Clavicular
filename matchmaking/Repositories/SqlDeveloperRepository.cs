using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class SqlDeveloperRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public Developer? GetById(int developerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT DeveloperId, Name, Password FROM Developer WHERE DeveloperId = @DeveloperId",
            connection);
        command.Parameters.AddWithValue("@DeveloperId", developerId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IReadOnlyList<Developer> GetAll()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT DeveloperId, Name, Password FROM Developer",
            connection);
        using var reader = command.ExecuteReader();

        var result = new List<Developer>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Developer developer)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Developer (Name, Password) VALUES (@Name, @Password)",
            connection);
        command.Parameters.AddWithValue("@Name", developer.Name);
        command.Parameters.AddWithValue("@Password", developer.Password);
        command.ExecuteNonQuery();
    }

    public void Update(Developer developer)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Developer SET Name = @Name, Password = @Password WHERE DeveloperId = @DeveloperId",
            connection);
        command.Parameters.AddWithValue("@DeveloperId", developer.DeveloperId);
        command.Parameters.AddWithValue("@Name", developer.Name);
        command.Parameters.AddWithValue("@Password", developer.Password);
        command.ExecuteNonQuery();
    }

    public void Remove(int developerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "DELETE FROM Developer WHERE DeveloperId = @DeveloperId",
            connection);
        command.Parameters.AddWithValue("@DeveloperId", developerId);
        command.ExecuteNonQuery();
    }

    private static Developer Map(SqlDataReader reader)
    {
        return new Developer
        {
            DeveloperId = reader.GetInt32(0),
            Name = reader.GetString(1),
            Password = reader.GetString(2)
        };
    }
}
