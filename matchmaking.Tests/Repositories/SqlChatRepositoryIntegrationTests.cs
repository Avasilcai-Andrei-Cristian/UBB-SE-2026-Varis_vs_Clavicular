namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlChatRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlChatRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void InsertSelectAndUpdatePaths_WhenChatRoundTrip_ShouldPersistAndMapFields()
    {
        var repository = new SqlChatRepository(database.ConnectionString);
        var chat = new Chat { UserId = 10, CompanyId = 70, JobId = 5, IsBlocked = false };

        repository.Add(chat);

        var byUserCompany = repository.GetByUserAndCompany(10, 70, 5);
        byUserCompany.Should().NotBeNull();
        byUserCompany!.ChatId.Should().Be(chat.ChatId);

        repository.BlockChat(chat.ChatId, 10);
        repository.GetChatById(chat.ChatId).IsBlocked.Should().BeTrue();

        repository.UnblockUser(chat.ChatId, 10);
        repository.GetChatById(chat.ChatId).IsBlocked.Should().BeFalse();

        repository.DeletedByUser(chat.ChatId, 10);
        repository.GetChatById(chat.ChatId).DeletedAtByUser.Should().NotBeNull();

        repository.DeletedBySecondParty(chat.ChatId, 70);
        repository.GetChatById(chat.ChatId).DeletedAtBySecondParty.Should().NotBeNull();
    }

    [Fact]
    public void QueryFilteringAndTimestampQueries_WhenChatsAndMessagesExist_ShouldReturnExpectedRows()
    {
        var chatRepository = new SqlChatRepository(database.ConnectionString);
        var first = new Chat { UserId = 1, SecondUserId = 2 };
        var second = new Chat { UserId = 1, SecondUserId = 3 };
        var companyChat = new Chat { UserId = 4, CompanyId = 99, JobId = 20 };
        chatRepository.Add(first);
        chatRepository.Add(second);
        chatRepository.Add(companyChat);

        var byUser = chatRepository.GetByUserId(1);
        byUser.Should().HaveCount(2);
        byUser[0].ChatId.Should().BeGreaterThan(byUser[1].ChatId);

        var pair = chatRepository.GetByUsers(2, 1);
        pair.Should().NotBeNull();
        pair!.ChatId.Should().Be(first.ChatId);
        chatRepository.GetByCompanyId(99).Should().ContainSingle().Which.ChatId.Should().Be(companyChat.ChatId);

        database.ExecuteNonQuery(
            "INSERT INTO Message (Content, SenderID, Timestamp, ChatId, Type, IsRead) VALUES (@Content, @Sender, @Timestamp, @ChatId, @Type, @IsRead)",
            parameters =>
            {
                parameters.AddWithValue("@Content", "latest");
                parameters.AddWithValue("@Sender", 1);
                parameters.AddWithValue("@Timestamp", new DateTime(2026, 03, 01, 10, 0, 0, DateTimeKind.Utc));
                parameters.AddWithValue("@ChatId", first.ChatId);
                parameters.AddWithValue("@Type", (byte)MessageType.Text);
                parameters.AddWithValue("@IsRead", false);
            });

        var latestMap = chatRepository.GetLatestMessageTimestamps([first.ChatId, second.ChatId]);
        latestMap.Should().ContainKey(first.ChatId);
        latestMap[first.ChatId].Should().Be(new DateTime(2026, 03, 01, 10, 0, 0, DateTimeKind.Utc));
        latestMap.Should().NotContainKey(second.ChatId);
    }
}
