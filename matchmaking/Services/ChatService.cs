using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using matchmaking.Domain.Enums;

namespace matchmaking.Services;

public class ChatService
{
    private readonly SqlChatRepository _chatRepository;
    private readonly SqlMessageRepository _messageRepository;
    private readonly UserRepository _userRepository;
    private readonly CompanyRepository _companyRepository;

    public ChatService(
        SqlChatRepository chatRepo,
        SqlMessageRepository messageRepo,
        UserRepository userRepo,
        CompanyRepository companyRepo)
    {
        _chatRepository = chatRepo;
        _messageRepository = messageRepo;
        _userRepository = userRepo;
        _companyRepository = companyRepo;
    }

    public Chat FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId = null)
    {
        var existingChat = _chatRepository.GetByUserAndCompany(userId, companyId);
        if (existingChat is not null)
        {
            return existingChat;
        }

        var chat = new Chat
        {
            UserId = userId,
            CompanyId = companyId,
            SecondUserId = null,
            JobId = jobId,
            IsBlocked = false,
            IsDeletedByUser = false,
            IsDeletedBySecondParty = false
        };

        _chatRepository.Add(chat);
        return chat;
    }

    public Chat FindOrCreateUserUserChat(int userId, int secondUserId)
    {
        var existingChat = _chatRepository.GetByUsers(userId, secondUserId);
        if (existingChat is not null)
        {
            return existingChat;
        }
        var chat = new Chat
        {
            UserId = userId,
            CompanyId = null,
            SecondUserId = secondUserId,
            JobId = null,
            IsBlocked = false,
            IsDeletedByUser = false,
            IsDeletedBySecondParty = false
        };
        _chatRepository.Add(chat);
        return chat;
    }

    public List<Chat> GetChatsForUser(int userId)
    {
        return _chatRepository.GetByUserId(userId).ToList();
    }

    public List<Chat> GetChatsForCompany(int companyId)
    {
        return _chatRepository.GetByCompanyId(companyId).ToList();
    }

    public List<Message> GetMessages(int chatId)
    {
        return _messageRepository.GetByChatId(chatId).ToList();
    }

    public List<Company> SearchCompanies(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Company>();

        return _companyRepository.GetAll()
            .Where(c => c.CompanyName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<User> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<User>();
        return _userRepository.GetAll()
            .Where(u => u.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void SendMessage(int chatId, string content, int senderId, MessageType type)
    {
        if (string.IsNullOrEmpty(content))
            throw new ArgumentException("Message content cannot be empty.");

        Chat chat = _chatRepository.GetChatById(chatId);
        if (chat.IsBlocked)
            throw new InvalidOperationException("Cannot send message in a blocked chat.");

        if (MessageType.Text == type && content.Length > 2000)
            throw new ArgumentException("Text messages cannot exceed 2000 characters.");

        if (MessageType.Image == type && !content.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
            !content.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) &&
            !content.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Image messages must be a URL ending with .jpg, .jpeg or .png");

        if (MessageType.Image == type && content.Length > 10 * 1024 * 1024)
            throw new InvalidOperationException("Image must be less than 10 MB");

        if (MessageType.File == type && !content.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
            !content.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) &&
            !content.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("File messages must be a URL ending with .pdf, .docx or doc");

        if (MessageType.File == type && content.Length > 20 * 1024 * 1024)
            throw new InvalidOperationException("File must be less than 20 MB");

        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            Type = type,
            IsRead = false
        };

        _messageRepository.Add(message);
    }


}
