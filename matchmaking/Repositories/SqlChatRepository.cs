using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class SqlChatRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public IReadOnlyList<Chat> GetByUserId(int userId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, IsDeletedByUser, IsDeletedBySecondParty FROM Chat WHERE (UserId = @UserId OR SecondUserId = @UserId) AND IsDeletedByUser = 0 ORDER BY ISNULL((SELECT MAX(m.[Timestamp]) FROM Message m WHERE m.ChatId = Chat.ChatId), '19000101') DESC, ChatId DESC",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();
        var result = new List<Chat>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public IReadOnlyList<Chat> GetByCompanyId(int companyId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, IsDeletedByUser, IsDeletedBySecondParty FROM Chat WHERE CompanyId = @CompanyId AND IsDeletedBySecondParty = 0 ORDER BY ISNULL((SELECT MAX(m.[Timestamp]) FROM Message m WHERE m.ChatId = Chat.ChatId), '19000101') DESC, ChatId DESC",
            connection);
        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = command.ExecuteReader();
        var result = new List<Chat>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public Chat? GetByUsers(int userId, int secondUserId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, IsDeletedByUser, IsDeletedBySecondParty FROM Chat WHERE (UserId = @UserId AND SecondUserId = @SecondUserId) OR (UserId = @SecondUserId AND SecondUserId = @UserId)",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@SecondUserId", secondUserId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public Chat? GetByUserAndCompany(int userId, int companyId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, IsDeletedByUser, IsDeletedBySecondParty FROM Chat WHERE UserId = @UserId AND CompanyId = @CompanyId",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@CompanyId", companyId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public Chat GetChatById(int chatId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, IsDeletedByUser, IsDeletedBySecondParty FROM Chat WHERE ChatId = @ChatId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : throw new KeyNotFoundException($"Chat with id {chatId} was not found.");
    }

    public void Add(Chat chat)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Chat (UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, IsDeletedByUser, IsDeletedBySecondParty) OUTPUT INSERTED.ChatId VALUES (@UserId, @CompanyId, @SecondUserId, @JobId, @IsBlocked, @BlockedByUserId, @IsDeletedByUser, @IsDeletedBySecondParty)",
            connection);
        command.Parameters.AddWithValue("@UserId", chat.UserId);
        command.Parameters.AddWithValue("@CompanyId", (object?)chat.CompanyId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SecondUserId", (object?)chat.SecondUserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@JobId", (object?)chat.JobId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsBlocked", chat.IsBlocked);
        command.Parameters.AddWithValue("@BlockedByUserId", (object?)chat.BlockedByUserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsDeletedByUser", chat.IsDeletedByUser);
        command.Parameters.AddWithValue("@IsDeletedBySecondParty", chat.IsDeletedBySecondParty);

        chat.ChatId = Convert.ToInt32(command.ExecuteScalar());
    }

    public void BlockChat(int chatId, int blockerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET IsBlocked = 1, BlockedByUserId = @BlockedByUserId WHERE ChatId = @ChatId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@BlockedByUserId", blockerId);
        command.ExecuteNonQuery();
    }

    public void UnblockUser(int chatId, int requesterId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET IsBlocked = 0, BlockedByUserId = NULL WHERE ChatId = @ChatId AND BlockedByUserId = @RequesterId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@RequesterId", requesterId);
        command.ExecuteNonQuery();
    }

    public void DeletedByUser(int chatId, int userId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET IsDeletedByUser = 1 WHERE ChatId = @ChatId AND UserId = @UserId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@UserId", userId);
        command.ExecuteNonQuery();
    }

    public void DeletedBySecondParty(int chatId, int callerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET IsDeletedBySecondParty = 1 WHERE ChatId = @ChatId AND (SecondUserId = @CallerId OR CompanyId = @CallerId)",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@CallerId", callerId);
        command.ExecuteNonQuery();
    }

    public void RestoreDeletedByUser(int chatId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET IsDeletedByUser = 0 WHERE ChatId = @ChatId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.ExecuteNonQuery();
    }

    public void RestoreDeletedBySecondParty(int chatId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET IsDeletedBySecondParty = 0 WHERE ChatId = @ChatId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.ExecuteNonQuery();
    }

    public void UpdateJobId(int chatId, int jobId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET JobId = @JobId WHERE ChatId = @ChatId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@JobId", jobId);
        command.ExecuteNonQuery();
    }

    private static Chat Map(SqlDataReader reader)
    {
        return new Chat
        {
            ChatId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            CompanyId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            SecondUserId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            JobId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            IsBlocked = reader.GetBoolean(5),
            BlockedByUserId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            IsDeletedByUser = reader.GetBoolean(7),
            IsDeletedBySecondParty = reader.GetBoolean(8)
        };
    }
}
