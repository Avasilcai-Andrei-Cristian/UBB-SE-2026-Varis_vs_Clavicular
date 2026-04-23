using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Services;

public interface IChatService
{
    Chat? FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId = null);
    Chat? FindOrCreateUserUserChat(int userId, int secondUserId);
    List<Chat> GetChatsForUser(int userId);
    List<Chat> GetChatsForCompany(int companyId);
    List<Message> GetMessages(int chatId, int callerId);
    List<Company> SearchCompanies(string query);
    List<User> SearchUsers(string query);
    void SendMessage(int chatId, string content, int senderId, MessageType type);
    void MarkMessageAsRead(int chatId, int readerId);
    void BlockUser(int chatId, int blockerId);
    void UnblockUser(int chatId, int unblockerId);
    void DeleteChat(int chatId, int callerId);
}
