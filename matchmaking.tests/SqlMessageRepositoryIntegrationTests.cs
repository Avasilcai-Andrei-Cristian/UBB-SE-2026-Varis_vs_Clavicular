namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlMessageRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlMessageRepositoryIntegrationTests(SqlIntegrationTestDatabaseFixture fixture)
    {
        database = fixture.Database;
        database.ResetData();
    }

    [Fact]
    public void InsertSelectMapAndTimestampFiltering_WhenMessagesExist_ShouldReturnOrderedRows()
    {
        var chatId = InsertChat(userId: 100, companyId: 20, jobId: 9);
        var repository = new SqlMessageRepository(database.ConnectionString);

        repository.Add(new Message { Content = "first", SenderId = 100, ChatId = chatId, Type = MessageType.Text, IsRead = false });
        repository.Add(new Message { Content = "second", SenderId = 20, ChatId = chatId, Type = MessageType.Image, IsRead = false });

        var allMessages = repository.GetByChatId(chatId);
        allMessages.Should().HaveCount(2);
        allMessages[0].Content.Should().Be("first");
        allMessages[1].Type.Should().Be(MessageType.Image);

        repository.GetByChatId(chatId, allMessages[1].Timestamp.AddSeconds(1)).Should().BeEmpty();
    }

    [Fact]
    public void UpdatePath_WhenMarkAsReadCalled_ShouldOnlyMarkIncomingMessages()
    {
        var chatId = InsertChat(userId: 7, secondUserId: 8);
        var repository = new SqlMessageRepository(database.ConnectionString);
        repository.Add(new Message { Content = "mine", SenderId = 7, ChatId = chatId, Type = MessageType.Text, IsRead = false });
        repository.Add(new Message { Content = "theirs", SenderId = 8, ChatId = chatId, Type = MessageType.Text, IsRead = false });

        repository.MarkAsRead(chatId, 7);

        var allMessages = repository.GetByChatId(chatId);
        allMessages.Single(item => item.SenderId == 7).IsRead.Should().BeFalse();
        allMessages.Single(item => item.SenderId == 8).IsRead.Should().BeTrue();
    }

    private int InsertChat(int userId, int? companyId = null, int? secondUserId = null, int? jobId = null)
    {
        return database.ExecuteScalar<int>(
            "INSERT INTO Chat (UserId, CompanyId, SecondUserId, JobId, IsBlocked) VALUES (@UserId, @CompanyId, @SecondUserId, @JobId, 0); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            parameters =>
            {
                parameters.AddWithValue("@UserId", userId);
                parameters.AddWithValue("@CompanyId", (object?)companyId ?? DBNull.Value);
                parameters.AddWithValue("@SecondUserId", (object?)secondUserId ?? DBNull.Value);
                parameters.AddWithValue("@JobId", (object?)jobId ?? DBNull.Value);
            });
    }
}
