using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class SqlChatRepository : SqlRepositoryBase, IChatRepository
{
    public SqlChatRepository(string connectionString)
        : base(connectionString)
    {
    }

    public IReadOnlyList<Chat> GetByUserId(int userId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            @"SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, 
                 DeletedAtByUser, DeletedAtBySecondParty 
          FROM Chat 
          WHERE UserId = @UserId OR SecondUserId = @UserId
          ORDER BY ChatId DESC",
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
            @"SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, 
                 DeletedAtByUser, DeletedAtBySecondParty 
          FROM Chat 
          WHERE CompanyId = @CompanyId
          ORDER BY ChatId DESC",
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

    public IReadOnlyDictionary<int, DateTime?> GetLatestMessageTimestamps(IEnumerable<int> chatIds)
    {
        var ids = string.Join(",", chatIds);
        if (string.IsNullOrEmpty(ids))
        {
            return new Dictionary<int, DateTime?>();
        }

        using var connection = OpenConnection();
        using var command = new SqlCommand(
            $@"SELECT ChatId, MAX([Timestamp]) 
           FROM Message 
           WHERE ChatId IN ({ids}) 
           GROUP BY ChatId",
            connection);

        using var reader = command.ExecuteReader();
        var result = new Dictionary<int, DateTime?>();
        while (reader.Read())
        {
            result[reader.GetInt32(0)] = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
        }
        return result;
    }

    public Chat? GetByUsers(int userId, int secondUserId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, DeletedAtByUser, DeletedAtBySecondParty FROM Chat WHERE (UserId = @UserId AND SecondUserId = @SecondUserId) OR (UserId = @SecondUserId AND SecondUserId = @UserId)",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@SecondUserId", secondUserId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public Chat? GetByUserAndCompany(int userId, int companyId, int? jobId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, DeletedAtByUser, DeletedAtBySecondParty FROM Chat WHERE UserId = @UserId AND CompanyId = @CompanyId AND ((@JobId IS NULL AND JobId IS NULL) OR JobId = @JobId)",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@JobId", (object?)jobId ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public Chat GetChatById(int chatId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT ChatId, UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, DeletedAtByUser, DeletedAtBySecondParty FROM Chat WHERE ChatId = @ChatId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : throw new KeyNotFoundException($"Chat with id {chatId} was not found.");
    }

    public void Add(Chat chat)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Chat (UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, DeletedAtByUser, DeletedAtBySecondParty) OUTPUT INSERTED.ChatId VALUES (@UserId, @CompanyId, @SecondUserId, @JobId, @IsBlocked, @BlockedByUserId, @DeletedAtByUser, @DeletedAtBySecondParty)",
            connection);
        command.Parameters.AddWithValue("@UserId", chat.UserId);
        command.Parameters.AddWithValue("@CompanyId", (object?)chat.CompanyId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SecondUserId", (object?)chat.SecondUserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@JobId", (object?)chat.JobId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsBlocked", chat.IsBlocked);
        command.Parameters.AddWithValue("@BlockedByUserId", (object?)chat.BlockedByUserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@DeletedAtByUser", (object?)chat.DeletedAtByUser ?? DBNull.Value);
        command.Parameters.AddWithValue("@DeletedAtBySecondParty", (object?)chat.DeletedAtBySecondParty ?? DBNull.Value);

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
            "UPDATE Chat SET DeletedAtByUser = SYSUTCDATETIME() WHERE ChatId = @ChatId AND UserId = @UserId",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@UserId", userId);
        command.ExecuteNonQuery();
    }

    public void DeletedBySecondParty(int chatId, int callerId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Chat SET DeletedAtBySecondParty = SYSUTCDATETIME() WHERE ChatId = @ChatId AND (SecondUserId = @CallerId OR CompanyId = @CallerId)",
            connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@CallerId", callerId);
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
            DeletedAtByUser = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            DeletedAtBySecondParty = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
    }
}
