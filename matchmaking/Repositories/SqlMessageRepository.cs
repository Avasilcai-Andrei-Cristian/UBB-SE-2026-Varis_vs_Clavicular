using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Repositories;

public class SqlMessageRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public IReadOnlyList<Message> GetByChatId(int chatId, DateTime? visibleAfter = null)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MessageId, Content, SenderId, Timestamp, ChatId, Type, IsRead FROM Message WHERE ChatId = @ChatId AND (@VisibleAfter IS NULL OR [Timestamp] >= @VisibleAfter) ORDER BY Timestamp",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@VisibleAfter", (object?)visibleAfter ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        var result = new List<Message>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Message message)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Message (Content, SenderId, Timestamp, ChatId, Type, IsRead) VALUES ( @Content, @SenderId, SYSUTCDATETIME(), @ChatId, @Type, @IsRead)",
            connection);
        command.Parameters.AddWithValue("@Content", message.Content);
        command.Parameters.AddWithValue("@SenderId", message.SenderId);
        command.Parameters.AddWithValue("@ChatId", message.ChatId);
        command.Parameters.AddWithValue("@Type", (int)message.Type);
        command.Parameters.AddWithValue("@IsRead", message.IsRead);
        command.ExecuteNonQuery();
    }

    public void MarkAsRead(int chatId, int readerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Message SET IsRead = 1 WHERE ChatId = @ChatId AND SenderId <> @ReaderId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@ReaderId", readerId);
        command.ExecuteNonQuery();
    }

    private static Message Map(SqlDataReader reader)
    {
        return new Message
        {
            MessageId = reader.GetInt32(0),
            Content = reader.GetString(1),
            SenderId = reader.GetInt32(2),
            Timestamp = reader.GetDateTime(3),
            ChatId = reader.GetInt32(4),
            Type = (MessageType)Convert.ToInt32(reader.GetValue(5)),
            IsRead = reader.GetBoolean(6)
        };
    }
}
