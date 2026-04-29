using System.Linq;

namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlDeveloperRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlDeveloperRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Add_inserts_developer_and_get_by_id_retrieves_it()
    {
        var repository = new SqlDeveloperRepository(database.ConnectionString);
        var newDeveloper = new Developer { Name = "alice_dev", Password = "securepass1" };
        repository.Add(newDeveloper);
        var all = repository.GetAll();
        all.Should().ContainSingle();
        all[0].Name.Should().Be("alice_dev");
    }

    [Fact]
    public void Update_modifies_developer_name_and_password()
    {
        var repository = new SqlDeveloperRepository(database.ConnectionString);
        var originalDeveloper = new Developer { Name = "bob_dev", Password = "oldpassword" };
        repository.Add(originalDeveloper);
        var insertedDeveloper = repository.GetAll().Single();
        repository.Update(new Developer
        {
            DeveloperId = insertedDeveloper.DeveloperId,
            Name = "bob_dev_updated",
            Password = "newpassword"
        });
        var retrieved = repository.GetById(insertedDeveloper.DeveloperId);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("bob_dev_updated");
        retrieved.Password.Should().Be("newpassword");
    }
}