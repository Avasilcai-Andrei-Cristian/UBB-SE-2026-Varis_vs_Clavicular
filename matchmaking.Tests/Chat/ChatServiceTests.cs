using System.Collections.Generic;
using System.IO;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class ChatServiceTests
{
    [Fact]
    public void FindOrCreateUserCompanyChat_returns_null_when_blocked_by_other_party()
    {
        var chatRepository = new FakeChatRepository();
        chatRepository.UserChats =
        [
            new Chat { ChatId = 1, UserId = 10, CompanyId = 20, IsBlocked = true, BlockedByUserId = 999 }
        ];
        var service = CreateService(chatRepository);

        var result = service.FindOrCreateUserCompanyChat(10, 20, null);

        result.Should().BeNull();
        chatRepository.AddedChats.Should().BeEmpty();
    }

    [Fact]
    public void FindOrCreateUserCompanyChat_returns_existing_chat_when_found()
    {
        var existing = new Chat { ChatId = 7, UserId = 1, CompanyId = 2, JobId = 3 };
        var chatRepository = new FakeChatRepository { ExistingUserCompanyChat = existing };
        var service = CreateService(chatRepository);

        var result = service.FindOrCreateUserCompanyChat(1, 2, 3);

        result.Should().BeSameAs(existing);
        chatRepository.AddedChats.Should().BeEmpty();
    }

    [Fact]
    public void FindOrCreateUserCompanyChat_creates_chat_when_missing()
    {
        var chatRepository = new FakeChatRepository();
        var service = CreateService(chatRepository);

        var result = service.FindOrCreateUserCompanyChat(3, 4, 5);

        result.Should().NotBeNull();
        chatRepository.AddedChats.Should().ContainSingle();
        chatRepository.AddedChats[0].UserId.Should().Be(3);
        chatRepository.AddedChats[0].CompanyId.Should().Be(4);
        chatRepository.AddedChats[0].JobId.Should().Be(5);
        chatRepository.AddedChats[0].IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void FindOrCreateUserUserChat_returns_existing_chat_when_found()
    {
        var existing = new Chat { ChatId = 4, UserId = 1, SecondUserId = 2 };
        var chatRepository = new FakeChatRepository { ExistingUserUserChat = existing };
        var service = CreateService(chatRepository);

        var result = service.FindOrCreateUserUserChat(1, 2);

        result.Should().BeSameAs(existing);
        chatRepository.AddedChats.Should().BeEmpty();
    }

    [Fact]
    public void FindOrCreateUserUserChat_returns_null_when_blocked_by_other_user()
    {
        var chatRepository = new FakeChatRepository();
        chatRepository.UserChats =
        [
            new Chat { ChatId = 11, UserId = 10, SecondUserId = 55, IsBlocked = true, BlockedByUserId = 55 }
        ];
        var service = CreateService(chatRepository);

        var result = service.FindOrCreateUserUserChat(10, 55);

        result.Should().BeNull();
        chatRepository.AddedChats.Should().BeEmpty();
    }

    [Fact]
    public void SendMessage_returns_without_adding_when_blocked_by_other_party()
    {
        var chatRepository = new FakeChatRepository();
        chatRepository.ChatById = new Chat { ChatId = 1, IsBlocked = true, BlockedByUserId = 99 };
        var messageRepository = new FakeMessageRepository();
        var service = CreateService(chatRepository, messageRepository);

        service.SendMessage(1, "hi", senderId: 10, MessageType.Text);

        messageRepository.AddedMessages.Should().BeEmpty();
    }

    [Fact]
    public void SendMessage_throws_when_content_is_empty()
    {
        var service = CreateService(new FakeChatRepository());

        Action act = () => service.SendMessage(1, " ", senderId: 10, MessageType.Text);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SendMessage_adds_message_when_allowed()
    {
        var chatRepository = new FakeChatRepository();
        chatRepository.ChatById = new Chat { ChatId = 7, IsBlocked = false };
        var messageRepository = new FakeMessageRepository();
        var service = CreateService(chatRepository, messageRepository);

        service.SendMessage(7, "hello", senderId: 42, MessageType.Text);

        messageRepository.AddedMessages.Should().ContainSingle();
        messageRepository.AddedMessages[0].ChatId.Should().Be(7);
        messageRepository.AddedMessages[0].SenderId.Should().Be(42);
        messageRepository.AddedMessages[0].Content.Should().Be("hello");
        messageRepository.AddedMessages[0].Type.Should().Be(MessageType.Text);
        messageRepository.AddedMessages[0].IsRead.Should().BeFalse();
    }

    [Fact]
    public void MarkMessageAsRead_delegates_to_repository()
    {
        var messageRepository = new FakeMessageRepository();
        var service = CreateService(new FakeChatRepository(), messageRepository);

        service.MarkMessageAsRead(4, 12);

        messageRepository.MarkedRead.Should().ContainSingle().Which.Should().Be((4, 12));
    }

    [Fact]
    public void GetChatsForUser_propagates_repository_error()
    {
        var chatRepository = new FakeChatRepository { ThrowOnGetByUserId = new InvalidOperationException("boom") };
        var service = CreateService(chatRepository);

        Action act = () => service.GetChatsForUser(1);

        act.Should().Throw<InvalidOperationException>();
    }

    private static ChatService CreateService(
        FakeChatRepository chatRepository,
        FakeMessageRepository? messageRepository = null)
    {
        return new ChatService(
            chatRepository,
            messageRepository ?? new FakeMessageRepository(),
            new FakeUserRepository(),
            new FakeCompanyRepository(),
            () => Path.Combine(Path.GetTempPath(), "matchmaking-tests"));
    }

    private sealed class FakeChatRepository : IChatRepository
    {
        public List<Chat> UserChats { get; set; } = [];
        public List<Chat> CompanyChats { get; set; } = [];
        public Chat? ExistingUserCompanyChat { get; set; }
        public Chat? ExistingUserUserChat { get; set; }
        public Chat? ChatById { get; set; }
        public Dictionary<int, DateTime?> LatestTimestamps { get; set; } = new();
        public List<Chat> AddedChats { get; } = [];
        public Exception? ThrowOnGetByUserId { get; set; }

        public Chat? GetByUserAndCompany(int userId, int companyId, int? jobId = null) => ExistingUserCompanyChat;
        public Chat? GetByUsers(int userId, int secondUserId) => ExistingUserUserChat;
        public IReadOnlyList<Chat> GetByUserId(int userId)
        {
            if (ThrowOnGetByUserId is not null)
            {
                throw ThrowOnGetByUserId;
            }

            return UserChats;
        }
        public IReadOnlyList<Chat> GetByCompanyId(int companyId) => CompanyChats;
        public Chat GetChatById(int chatId) => ChatById ?? throw new KeyNotFoundException();
        public IReadOnlyDictionary<int, DateTime?> GetLatestMessageTimestamps(IEnumerable<int> chatIds) => LatestTimestamps;
        public void Add(Chat chat) => AddedChats.Add(chat);
        public void BlockChat(int chatId, int blockerId) { }
        public void UnblockUser(int chatId, int unblockerId) { }
        public void DeletedByUser(int chatId, int userId) { }
        public void DeletedBySecondParty(int chatId, int secondPartyId) { }
    }

    private sealed class FakeMessageRepository : IMessageRepository
    {
        public List<Message> AddedMessages { get; } = [];
        public List<(int ChatId, int ReaderId)> MarkedRead { get; } = [];

        public IReadOnlyList<Message> GetByChatId(int chatId, DateTime? visibleAfter = null) => [];
        public void Add(Message message) => AddedMessages.Add(message);
        public void MarkAsRead(int chatId, int readerId) => MarkedRead.Add((chatId, readerId));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? GetById(int userId) => null;
        public IReadOnlyList<User> GetAll() => [];
        public void Add(User user) { }
        public void Update(User user) { }
        public void Remove(int userId) { }
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        public Company? GetById(int companyId) => null;
        public IReadOnlyList<Company> GetAll() => [];
        public void Add(Company company) { }
        public void Update(Company company) { }
        public void Remove(int companyId) { }
    }
}
