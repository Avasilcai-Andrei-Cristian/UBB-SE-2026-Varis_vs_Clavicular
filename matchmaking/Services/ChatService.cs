using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using matchmaking.Domain.Enums;

namespace matchmaking.Services;

public class ChatService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc"
    };

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

    public Chat? FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId = null)
    {
        var blockedConversation = _chatRepository
            .GetByUserId(userId)
            .FirstOrDefault(chat =>
                chat.CompanyId == companyId
                && chat.IsBlocked
                && chat.BlockedByUserId != userId);

        if (blockedConversation is not null)
        {
            return null;
        }

        var existingChat = _chatRepository.GetByUserAndCompany(userId, companyId, jobId);
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
            IsBlocked = false
        };

        _chatRepository.Add(chat);
        return chat;
    }

    public Chat? FindOrCreateUserUserChat(int userId, int secondUserId)
    {
        var existingChat = _chatRepository.GetByUsers(userId, secondUserId);
        if (existingChat is not null)
        {
            return existingChat;
        }

        var blockedConversation = _chatRepository
            .GetByUserId(userId)
            .FirstOrDefault(chat =>
                (chat.UserId == secondUserId || chat.SecondUserId == secondUserId)
                && chat.IsBlocked
                && chat.BlockedByUserId != userId);

        if (blockedConversation is not null)
        {
            return null;
        }

        var chat = new Chat
        {
            UserId = userId,
            CompanyId = null,
            SecondUserId = secondUserId,
            JobId = null,
            IsBlocked = false
        };
        _chatRepository.Add(chat);
        return chat;
    }

    public List<Chat> GetChatsForUser(int userId)
    {
        var chats = _chatRepository.GetByUserId(userId);
        var timestamps = _chatRepository.GetLatestMessageTimestamps(chats.Select(c => c.ChatId));

        return chats
            .Where(c =>
            {
                if (c.IsBlocked && c.BlockedByUserId != userId)
                {
                    return false;
                }

                DateTime? deletedAt = c.UserId == userId
                    ? c.DeletedAtByUser
                    : c.DeletedAtBySecondParty;

                return deletedAt is null
                    || (timestamps.TryGetValue(c.ChatId, out var lastMsg) && lastMsg > deletedAt);
            })
            .OrderByDescending(c =>
                timestamps.TryGetValue(c.ChatId, out var ts) ? ts : (DateTime?)new DateTime(1900, 1, 1))
            .ThenByDescending(c => c.ChatId)
            .ToList();
    }

    public List<Chat> GetChatsForCompany(int companyId)
    {
        var chats = _chatRepository.GetByCompanyId(companyId);
        var timestamps = _chatRepository.GetLatestMessageTimestamps(chats.Select(c => c.ChatId));

        return chats
            .Where(c =>
            {
                if (c.IsBlocked && c.BlockedByUserId != companyId)
                {
                    return false;
                }

                return c.DeletedAtBySecondParty is null
                    || (timestamps.TryGetValue(c.ChatId, out var lastMsg) && lastMsg > c.DeletedAtBySecondParty);
            })
            .OrderByDescending(c =>
                timestamps.TryGetValue(c.ChatId, out var ts) ? ts : (DateTime?)new DateTime(1900, 1, 1))
            .ThenByDescending(c => c.ChatId)
            .ToList();
    }

    public List<Message> GetMessages(int chatId, int callerId)
    {
        var chat = _chatRepository.GetChatById(chatId);

        if (chat.UserId != callerId && chat.SecondUserId != callerId && chat.CompanyId != callerId)
            throw new UnauthorizedAccessException("Only participants can access messages.");

        DateTime? visibleAfter = chat.UserId == callerId
            ? chat.DeletedAtByUser
            : chat.DeletedAtBySecondParty;

        return _messageRepository.GetByChatId(chatId, visibleAfter).ToList();
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
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.");

        Chat chat = _chatRepository.GetChatById(chatId);
        if (chat.IsBlocked && chat.BlockedByUserId != senderId)
        {
            return;
        }

        if (chat.IsBlocked)
            throw new InvalidOperationException("Cannot send message in a blocked chat.");

        if (MessageType.Text == type && content.Length > 2000)
            throw new ArgumentException("Text messages cannot exceed 2000 characters.");

        if (type == MessageType.Image || type == MessageType.File)
        {
            content = StoreAttachment(content, type);
        }

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

    private static string StoreAttachment(string sourcePath, MessageType type)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Attachment file was not found.", sourcePath);

        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
            throw new NotSupportedException("Attachment must have a valid file extension.");

        if (type == MessageType.Image && !AllowedImageExtensions.Contains(extension))
            throw new NotSupportedException("Image messages must be .jpg, .jpeg or .png");

        if (type == MessageType.File && !AllowedFileExtensions.Contains(extension))
            throw new NotSupportedException("File messages must be .pdf, .docx or .doc");

        var fileInfo = new FileInfo(sourcePath);
        var maxBytes = type == MessageType.Image ? 10 * 1024 * 1024 : 20 * 1024 * 1024;
        if (fileInfo.Length > maxBytes)
            throw new InvalidOperationException(type == MessageType.Image
                ? "Image must be less than 10 MB"
                : "File must be less than 20 MB");

        var now = DateTime.UtcNow;
        var targetDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "matchmaking",
            "attachments",
            now.ToString("yyyy"),
            now.ToString("MM"),
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(targetDirectory);

        var targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePath));
        File.Copy(sourcePath, targetPath, overwrite: false);

        return targetPath;
    }

    public void MarkMessageAsRead(int chatId, int readerId) 
    {
        _messageRepository.MarkAsRead(chatId, readerId);
    }

    public void BlockUser(int chatId, int blockerId)
    {
        var chat = _chatRepository.GetChatById(chatId);
        if (chat.IsBlocked)
            throw new InvalidOperationException("Chat is already blocked.");
        _chatRepository.BlockChat(chatId, blockerId);
    }

    public void UnblockUser(int chatId, int unblockerId)
    {
        var chat = _chatRepository.GetChatById(chatId);
        if (!chat.IsBlocked)
            throw new InvalidOperationException("Chat is not blocked.");
        if (chat.BlockedByUserId != unblockerId)
            throw new UnauthorizedAccessException("Only the user who blocked the chat can unblock it.");
        _chatRepository.UnblockUser(chatId, unblockerId);
    }

    public void DeleteChat(int chatId, int callerId)
    {
        var chat = _chatRepository.GetChatById(chatId);
        if (chat.UserId != callerId && chat.SecondUserId != callerId && chat.CompanyId != callerId)
            throw new UnauthorizedAccessException("Only participants can delete the chat.");
        if (chat.UserId == callerId)
        {
            _chatRepository.DeletedByUser(chatId, callerId);
        }
        else if (chat.CompanyId == callerId || chat.SecondUserId == callerId)
        {
            _chatRepository.DeletedBySecondParty(chatId, callerId);
        }
    }


}
