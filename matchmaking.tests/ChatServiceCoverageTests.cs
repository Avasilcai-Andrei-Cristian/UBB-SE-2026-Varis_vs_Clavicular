using System.IO;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class ChatServiceCoverageTests
{
    [Fact]
    public void GetChatsForUser_WhenBlockedByOtherParty_ExcludesChat()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.GetChatsForUser(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChatsForCompany_WhenDeletedAndNoNewMessages_ExcludesChat()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 1, DeletedAtBySecondParty = DateTime.UtcNow };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.GetChatsForCompany(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetMessages_WhenCallerIsParticipant_ReturnsMessages()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var message = new Message { ChatId = 1, SenderId = 1, Content = "hello", Type = MessageType.Text, Timestamp = DateTime.UtcNow };
        var harness = CreateHarness(chats: new[] { chat }, messages: new[] { message });

        var result = harness.Service.GetMessages(1, 1);

        result.Should().ContainSingle(item => item.Content == "hello");
    }

    [Fact]
    public void SendMessage_WhenImageFileDoesNotExist_ThrowsFileNotFoundException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.SendMessage(1, @"C:\missing\image.png", 1, MessageType.Image);

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void SendMessage_WhenFileExtensionIsUnsupported_ThrowsNotSupportedException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, "test");

        try
        {
            Action act = () => harness.Service.SendMessage(1, path, 1, MessageType.File);

            act.Should().Throw<NotSupportedException>();
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SendMessage_WhenChatIsBlockedByOtherUser_DoesNothing()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.SendMessage(1, "hello", 1, MessageType.Text);

        harness.MessageRepository.AddedMessages.Should().BeEmpty();
    }

    [Fact]
    public void BlockUser_WhenChatAlreadyBlocked_ThrowsInvalidOperationException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.BlockUser(1, 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UnblockUser_WhenCallerIsNotBlocker_ThrowsUnauthorizedAccessException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.UnblockUser(1, 1);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void DeleteChat_WhenCallerIsParticipant_DelegatesDeletion()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.DeleteChat(1, 1);

        harness.ChatRepository.DeletedByUserCalls.Should().ContainSingle(call => call.ChatId == 1 && call.CallerId == 1);
    }

    private static ChatServiceHarness CreateHarness(IReadOnlyList<Chat>? chats = null, IReadOnlyList<Message>? messages = null)
    {
        var chatRepository = new FakeChatRepository(chats ?? Array.Empty<Chat>());
        var messageRepository = new FakeMessageRepository(messages ?? Array.Empty<Message>());
        var userRepository = new FakeUserRepository(new[] { TestDataFactory.CreateUser() });
        var companyRepository = new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany() });

        return new ChatServiceHarness(
            new ChatService(chatRepository, messageRepository, userRepository, companyRepository),
            chatRepository,
            messageRepository);
    }

    private sealed class ChatServiceHarness
    {
        public ChatServiceHarness(ChatService service, FakeChatRepository chatRepository, FakeMessageRepository messageRepository)
        {
            Service = service;
            ChatRepository = chatRepository;
            MessageRepository = messageRepository;
        }

        public ChatService Service { get; }
        public FakeChatRepository ChatRepository { get; }
        public FakeMessageRepository MessageRepository { get; }
    }

    private sealed class FakeChatRepository : IChatRepository
    {
        private readonly List<Chat> chats;

        public FakeChatRepository(IReadOnlyList<Chat> chats)
        {
            this.chats = chats.ToList();
        }

        public List<(int ChatId, int CallerId)> DeletedByUserCalls { get; } = new();

        public Chat GetChatById(int chatId) => chats.First(chat => chat.ChatId == chatId);
        public IReadOnlyList<Chat> GetByUserId(int userId) => chats.Where(chat => chat.UserId == userId || chat.SecondUserId == userId).ToList();
        public IReadOnlyList<Chat> GetByCompanyId(int companyId) => chats.Where(chat => chat.CompanyId == companyId).ToList();
        public Chat? GetByUserAndCompany(int userId, int companyId, int? jobId = null) => chats.FirstOrDefault(chat => chat.UserId == userId && chat.CompanyId == companyId && chat.JobId == jobId);
        public Chat? GetByUsers(int userId, int secondUserId) => chats.FirstOrDefault(chat => chat.UserId == userId && chat.SecondUserId == secondUserId);
        public IReadOnlyDictionary<int, DateTime?> GetLatestMessageTimestamps(IEnumerable<int> chatIds) => chatIds.ToDictionary(id => id, _ => (DateTime?)null);
        public void Add(Chat chat) => chats.Add(chat);
        public void BlockChat(int chatId, int blockerId) => chats.First(chat => chat.ChatId == chatId).IsBlocked = true;
        public void UnblockUser(int chatId, int unblockerId) => chats.First(chat => chat.ChatId == chatId).IsBlocked = false;
        public void DeletedByUser(int chatId, int userId) => DeletedByUserCalls.Add((chatId, userId));
        public void DeletedBySecondParty(int chatId, int secondPartyId) { }
    }

    private sealed class FakeMessageRepository : IMessageRepository
    {
        public FakeMessageRepository(IReadOnlyList<Message> messages)
        {
            Messages = messages.ToList();
        }

        public List<Message> Messages { get; }
        public List<Message> AddedMessages { get; } = new();

        public IReadOnlyList<Message> GetByChatId(int chatId, DateTime? visibleAfter = null) => Messages.Where(message => message.ChatId == chatId).ToList();
        public void Add(Message message) => AddedMessages.Add(message);
        public void MarkAsRead(int chatId, int readerId) { }
    }
}
