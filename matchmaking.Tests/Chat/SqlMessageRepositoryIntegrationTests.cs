using System.Linq;

namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlMessageRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlMessageRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Add_inserts_message_row()
    {
        var chatRepository = new SqlChatRepository(database.ConnectionString);
        var messageRepository = new SqlMessageRepository(database.ConnectionString);
        var chat = new Chat { UserId = 1, SecondUserId = 2, IsBlocked = false };
        chatRepository.Add(chat);

        messageRepository.Add(new Message
        {
            ChatId = chat.ChatId,
            Content = "Hello",
            SenderId = 1,
            Type = MessageType.Text,
            IsRead = false
        });

        var messages = messageRepository.GetByChatId(chat.ChatId);
        messages.Should().ContainSingle();
        messages[0].Content.Should().Be("Hello");
    }

    [Fact]
    public void GetByChatId_returns_messages_ordered_by_timestamp()
    {
        var chatRepository = new SqlChatRepository(database.ConnectionString);
        var messageRepository = new SqlMessageRepository(database.ConnectionString);
        var chat = new Chat { UserId = 3, SecondUserId = 4, IsBlocked = false };
        chatRepository.Add(chat);

        messageRepository.Add(new Message
        {
            ChatId = chat.ChatId,
            Content = "First",
            SenderId = 3,
            Type = MessageType.Text,
            IsRead = false
        });
        messageRepository.Add(new Message
        {
            ChatId = chat.ChatId,
            Content = "Second",
            SenderId = 4,
            Type = MessageType.Text,
            IsRead = false
        });

        var messages = messageRepository.GetByChatId(chat.ChatId);

        messages.Select(message => message.Content).Should().ContainInOrder("First", "Second");
    }

    [Fact]
    public void GetByChatId_filters_by_visible_after()
    {
        var chatRepository = new SqlChatRepository(database.ConnectionString);
        var messageRepository = new SqlMessageRepository(database.ConnectionString);
        var chat = new Chat { UserId = 5, SecondUserId = 6, IsBlocked = false };
        chatRepository.Add(chat);

        messageRepository.Add(new Message
        {
            ChatId = chat.ChatId,
            Content = "First",
            SenderId = 5,
            Type = MessageType.Text,
            IsRead = false
        });
        messageRepository.Add(new Message
        {
            ChatId = chat.ChatId,
            Content = "Second",
            SenderId = 6,
            Type = MessageType.Text,
            IsRead = false
        });

        var all = messageRepository.GetByChatId(chat.ChatId);
        var cutoff = all.Last().Timestamp;

        var messages = messageRepository.GetByChatId(chat.ChatId, cutoff);

        messages.Select(message => message.Content).Should().ContainSingle().Which.Should().Be("Second");
    }

    [Fact]
    public void Add_round_trips_message_type()
    {
        var chatRepository = new SqlChatRepository(database.ConnectionString);
        var messageRepository = new SqlMessageRepository(database.ConnectionString);
        var chat = new Chat { UserId = 7, SecondUserId = 8, IsBlocked = false };
        chatRepository.Add(chat);

        messageRepository.Add(new Message
        {
            ChatId = chat.ChatId,
            Content = "file",
            SenderId = 7,
            Type = MessageType.File,
            IsRead = false
        });

        var message = messageRepository.GetByChatId(chat.ChatId).Single();
        message.Type.Should().Be(MessageType.File);
    }
}
