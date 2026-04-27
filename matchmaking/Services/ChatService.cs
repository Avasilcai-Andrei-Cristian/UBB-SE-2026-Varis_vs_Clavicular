using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class ChatService : IChatService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc"
    };

    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly Func<string> _attachmentRootPathProvider;

    public ChatService(
        IChatRepository chatRepo,
        IMessageRepository messageRepo,
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        Func<string>? attachmentRootPathProvider = null)
    {
        _chatRepository = chatRepo;
        _messageRepository = messageRepo;
        _userRepository = userRepo;
        _companyRepository = companyRepo;
        _attachmentRootPathProvider = attachmentRootPathProvider ?? GetDefaultAttachmentRootPath;
    }

    public Chat? FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId = null)
    {
        foreach (var chat in _chatRepository.GetByUserId(userId))
        {
            if (IsBlockedCompanyChat(chat, companyId, userId))
            {
                return null;
            }
        }

        var existingChat = _chatRepository.GetByUserAndCompany(userId, companyId, jobId);
        if (existingChat is not null)
        {
            return existingChat;
        }

        var newChat = new Chat
        {
            UserId = userId,
            CompanyId = companyId,
            SecondUserId = null,
            JobId = jobId,
            IsBlocked = false
        };

        _chatRepository.Add(newChat);
        return newChat;
    }

    public Chat? FindOrCreateUserUserChat(int userId, int secondUserId)
    {
        var existingChat = _chatRepository.GetByUsers(userId, secondUserId);
        if (existingChat is not null)
        {
            return existingChat;
        }

        foreach (var chat in _chatRepository.GetByUserId(userId))
        {
            if (IsBlockedUserChat(chat, secondUserId, userId))
            {
                return null;
            }
        }

        var newChat = new Chat
        {
            UserId = userId,
            CompanyId = null,
            SecondUserId = secondUserId,
            JobId = null,
            IsBlocked = false
        };
        _chatRepository.Add(newChat);
        return newChat;
    }

    public List<Chat> GetChatsForUser(int userId)
    {
        var chats = _chatRepository.GetByUserId(userId);
        var timestamps = _chatRepository.GetLatestMessageTimestamps(GetChatIds(chats));

        var visibleChats = new List<Chat>();
        foreach (var chat in chats)
        {
            if (ShouldIncludeChatForUser(chat, userId, timestamps))
            {
                visibleChats.Add(chat);
            }
        }

        var comparer = new ChatTimestampComparer(timestamps);
        visibleChats.Sort(comparer.Compare);
        return visibleChats;
    }

    public List<Chat> GetChatsForCompany(int companyId)
    {
        var chats = _chatRepository.GetByCompanyId(companyId);
        var timestamps = _chatRepository.GetLatestMessageTimestamps(GetChatIds(chats));

        var visibleChats = new List<Chat>();
        foreach (var chat in chats)
        {
            if (ShouldIncludeChatForCompany(chat, companyId, timestamps))
            {
                visibleChats.Add(chat);
            }
        }

        var comparer = new ChatTimestampComparer(timestamps);
        visibleChats.Sort(comparer.Compare);
        return visibleChats;
    }

    public List<Message> GetMessages(int chatId, int callerId)
    {
        var chat = _chatRepository.GetChatById(chatId);

        if (chat.UserId != callerId && chat.SecondUserId != callerId && chat.CompanyId != callerId)
        {
            throw new UnauthorizedAccessException("Only participants can access messages.");
        }

        DateTime? visibleAfter = chat.UserId == callerId
            ? chat.DeletedAtByUser
            : chat.DeletedAtBySecondParty;

        return _messageRepository.GetByChatId(chatId, visibleAfter).ToList();
    }

    public List<Company> SearchCompanies(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<Company>();
        }

        var companies = new List<Company>();
        foreach (var company in _companyRepository.GetAll())
        {
            if (company.CompanyName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                companies.Add(company);
            }
        }

        return companies;
    }

    public List<User> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<User>();
        }

        var users = new List<User>();
        foreach (var user in _userRepository.GetAll())
        {
            if (user.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                users.Add(user);
            }
        }

        return users;
    }

    public void SendMessage(int chatId, string content, int senderId, MessageType type)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.");
        }

        Chat chat = _chatRepository.GetChatById(chatId);
        if (chat.IsBlocked && chat.BlockedByUserId != senderId)
        {
            return;
        }

        if (chat.IsBlocked)
        {
            throw new InvalidOperationException("Cannot send message in a blocked chat.");
        }

        if (MessageType.Text == type && content.Length > 2000)
        {
            throw new ArgumentException("Text messages cannot exceed 2000 characters.");
        }

        if (type == MessageType.Image || type == MessageType.File)
        {
            content = this.StoreAttachment(content, type);
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

    private string StoreAttachment(string sourcePath, MessageType type)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Attachment file was not found.", sourcePath);
        }

        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new NotSupportedException("Attachment must have a valid file extension.");
        }

        if (type == MessageType.Image && !AllowedImageExtensions.Contains(extension))
        {
            throw new NotSupportedException("Image messages must be .jpg, .jpeg or .png");
        }

        if (type == MessageType.File && !AllowedFileExtensions.Contains(extension))
        {
            throw new NotSupportedException("File messages must be .pdf, .docx or .doc");
        }

        var fileInfo = new FileInfo(sourcePath);
        var maxBytes = type == MessageType.Image ? 10 * 1024 * 1024 : 20 * 1024 * 1024;
        if (fileInfo.Length > maxBytes)
        {
            throw new InvalidOperationException(type == MessageType.Image
                ? "Image must be less than 10 MB"
                : "File must be less than 20 MB");
        }

        var now = DateTime.UtcNow;
        var targetDirectory = Path.Combine(
            _attachmentRootPathProvider(),
            now.ToString("yyyy"),
            now.ToString("MM"),
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(targetDirectory);

        var targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePath));
        File.Copy(sourcePath, targetPath, overwrite: false);

        return targetPath;
    }

    private static string GetDefaultAttachmentRootPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "matchmaking",
            "attachments");
    }

    public void MarkMessageAsRead(int chatId, int readerId)
    {
        _messageRepository.MarkAsRead(chatId, readerId);
    }

    public void BlockUser(int chatId, int blockerId)
    {
        var chat = _chatRepository.GetChatById(chatId);
        if (chat.IsBlocked)
        {
            throw new InvalidOperationException("Chat is already blocked.");
        }

        _chatRepository.BlockChat(chatId, blockerId);
    }

    public void UnblockUser(int chatId, int unblockerId)
    {
        var chat = _chatRepository.GetChatById(chatId);
        if (!chat.IsBlocked)
        {
            throw new InvalidOperationException("Chat is not blocked.");
        }

        if (chat.BlockedByUserId != unblockerId)
        {
            throw new UnauthorizedAccessException("Only the user who blocked the chat can unblock it.");
        }

        _chatRepository.UnblockUser(chatId, unblockerId);
    }

    public void DeleteChat(int chatId, int callerId)
    {
        var chat = _chatRepository.GetChatById(chatId);
        if (chat.UserId != callerId && chat.SecondUserId != callerId && chat.CompanyId != callerId)
        {
            throw new UnauthorizedAccessException("Only participants can delete the chat.");
        }

        if (chat.UserId == callerId)
        {
            _chatRepository.DeletedByUser(chatId, callerId);
        }
        else if (chat.CompanyId == callerId || chat.SecondUserId == callerId)
        {
            _chatRepository.DeletedBySecondParty(chatId, callerId);
        }
    }

    private static IEnumerable<int> GetChatIds(IEnumerable<Chat> chats)
    {
        var chatIds = new List<int>();
        foreach (var chat in chats)
        {
            chatIds.Add(chat.ChatId);
        }

        return chatIds;
    }

    private static bool IsBlockedCompanyChat(Chat chat, int companyId, int userId)
    {
        return chat.CompanyId == companyId
            && chat.IsBlocked
            && chat.BlockedByUserId != userId;
    }

    private static bool IsBlockedUserChat(Chat chat, int secondUserId, int userId)
    {
        return (chat.UserId == secondUserId || chat.SecondUserId == secondUserId)
            && chat.IsBlocked
            && chat.BlockedByUserId != userId;
    }

    private static bool ShouldIncludeChatForUser(Chat chat, int userId, IReadOnlyDictionary<int, DateTime?> timestamps)
    {
        if (chat.IsBlocked && chat.BlockedByUserId != userId)
        {
            return false;
        }

        DateTime? deletedAt = chat.UserId == userId
            ? chat.DeletedAtByUser
            : chat.DeletedAtBySecondParty;

        return deletedAt is null
            || (timestamps.TryGetValue(chat.ChatId, out var lastMessageTimestamp) && lastMessageTimestamp > deletedAt);
    }

    private static bool ShouldIncludeChatForCompany(Chat chat, int companyId, IReadOnlyDictionary<int, DateTime?> timestamps)
    {
        if (chat.IsBlocked && chat.BlockedByUserId != companyId)
        {
            return false;
        }

        return chat.DeletedAtBySecondParty is null
            || (timestamps.TryGetValue(chat.ChatId, out var lastMessageTimestamp) && lastMessageTimestamp > chat.DeletedAtBySecondParty);
    }

    private static DateTime ResolveChatTimestamp(int chatId, IReadOnlyDictionary<int, DateTime?> timestamps)
    {
        if (timestamps.TryGetValue(chatId, out var timestamp) && timestamp.HasValue)
        {
            return timestamp.Value;
        }

        return new DateTime(1900, 1, 1);
    }

    private sealed class ChatTimestampComparer
    {
        private readonly IReadOnlyDictionary<int, DateTime?> timestamps;

        public ChatTimestampComparer(IReadOnlyDictionary<int, DateTime?> timestamps)
        {
            this.timestamps = timestamps;
        }

        public int Compare(Chat left, Chat right)
        {
            var leftTimestamp = ResolveChatTimestamp(left.ChatId, timestamps);
            var rightTimestamp = ResolveChatTimestamp(right.ChatId, timestamps);

            var timestampComparison = rightTimestamp.CompareTo(leftTimestamp);
            if (timestampComparison != 0)
            {
                return timestampComparison;
            }

            return right.ChatId.CompareTo(left.ChatId);
        }
    }
}
