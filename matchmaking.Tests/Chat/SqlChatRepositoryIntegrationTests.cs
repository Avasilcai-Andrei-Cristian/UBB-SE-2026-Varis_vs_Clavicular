using System.Linq;

namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlChatRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlChatRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Add_creates_chat_and_assigns_id()
    {
        var repository = new SqlChatRepository(database.ConnectionString);
        var chat = new Chat { UserId = 10, CompanyId = 20, JobId = null, IsBlocked = false };

        repository.Add(chat);

        chat.ChatId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetByUserId_returns_chats_in_descending_id_order()
    {
        var repository = new SqlChatRepository(database.ConnectionString);
        var first = new Chat { UserId = 1, SecondUserId = 2, IsBlocked = false };
        var second = new Chat { UserId = 1, SecondUserId = 3, IsBlocked = false };
        repository.Add(first);
        repository.Add(second);

        var result = repository.GetByUserId(1);

        result.Select(item => item.ChatId).Should().ContainInOrder(second.ChatId, first.ChatId);
    }

    [Fact]
    public void GetByCompanyId_returns_chats_in_descending_id_order()
    {
        var repository = new SqlChatRepository(database.ConnectionString);
        var first = new Chat { UserId = 3, CompanyId = 8, IsBlocked = false };
        var second = new Chat { UserId = 4, CompanyId = 8, IsBlocked = false };
        repository.Add(first);
        repository.Add(second);

        var result = repository.GetByCompanyId(8);

        result.Select(item => item.ChatId).Should().ContainInOrder(second.ChatId, first.ChatId);
    }

    [Fact]
    public void GetByUserAndCompany_matches_job_id_when_provided()
    {
        var repository = new SqlChatRepository(database.ConnectionString);
        var chat = new Chat { UserId = 2, CompanyId = 3, JobId = 15, IsBlocked = false };
        repository.Add(chat);

        var result = repository.GetByUserAndCompany(2, 3, 15);

        result.Should().NotBeNull();
        result!.ChatId.Should().Be(chat.ChatId);
    }

    [Fact]
    public void DeletedByUser_sets_soft_delete_timestamp()
    {
        var repository = new SqlChatRepository(database.ConnectionString);
        var chat = new Chat { UserId = 5, SecondUserId = 6, IsBlocked = false };
        repository.Add(chat);

        repository.DeletedByUser(chat.ChatId, 5);

        var refreshed = repository.GetChatById(chat.ChatId);
        refreshed.DeletedAtByUser.Should().NotBeNull();
    }
}
