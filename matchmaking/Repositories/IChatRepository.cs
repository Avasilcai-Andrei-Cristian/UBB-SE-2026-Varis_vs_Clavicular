using System;
using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IChatRepository
{
    Chat? GetByUserAndCompany(int userId, int companyId, int? jobId = null);
    Chat? GetByUsers(int userId, int secondUserId);
    IReadOnlyList<Chat> GetByUserId(int userId);
    IReadOnlyList<Chat> GetByCompanyId(int companyId);
    Chat GetChatById(int chatId);
    IReadOnlyDictionary<int, DateTime?> GetLatestMessageTimestamps(IEnumerable<int> chatIds);
    void Add(Chat chat);
    void BlockChat(int chatId, int blockerId);
    void UnblockUser(int chatId, int unblockerId);
    void DeletedByUser(int chatId, int userId);
    void DeletedBySecondParty(int chatId, int secondPartyId);
}
