namespace matchmaking.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public void GetById_WhenUserExists_ReturnsUser()
    {
        var existingUser = TestDataFactory.CreateUser(7);
        var repository = new FakeUserRepository([existingUser]);
        var service = new UserService(repository);

        service.GetById(existingUser.UserId).Should().Be(existingUser);
    }

    [Fact]
    public void GetAll_WhenRepositoryHasUsers_ReturnsAllUsers()
    {
        var existingUser = TestDataFactory.CreateUser(7);
        var repository = new FakeUserRepository([existingUser]);
        var service = new UserService(repository);

        service.GetAll().Should().ContainSingle().Which.Should().Be(existingUser);
    }

    [Fact]
    public void Add_WhenUserAdded_DelegatesToRepository()
    {
        var repository = new FakeUserRepository(Array.Empty<User>());
        var service = new UserService(repository);
        var newUser = TestDataFactory.CreateUser(8);

        service.Add(newUser);

        repository.AddedUsers.Should().ContainSingle().Which.Should().Be(newUser);
    }

    [Fact]
    public void Update_WhenUserUpdated_DelegatesToRepository()
    {
        var existingUser = TestDataFactory.CreateUser(7);
        var repository = new FakeUserRepository([existingUser]);
        var service = new UserService(repository);

        service.Update(existingUser);

        repository.UpdatedUsers.Should().ContainSingle().Which.Should().Be(existingUser);
    }

    [Fact]
    public void Remove_WhenUserRemoved_DelegatesToRepository()
    {
        var existingUser = TestDataFactory.CreateUser(7);
        var repository = new FakeUserRepository([existingUser]);
        var service = new UserService(repository);

        service.Remove(existingUser.UserId);

        repository.RemovedUserIds.Should().ContainSingle().Which.Should().Be(existingUser.UserId);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> users;

        public FakeUserRepository(IReadOnlyList<User> users)
        {
            this.users = users.ToList();
        }

        public List<User> AddedUsers { get; } = new List<User>();
        public List<User> UpdatedUsers { get; } = new List<User>();
        public List<int> RemovedUserIds { get; } = new List<int>();

        public User? GetById(int userId) => users.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => users;
        public void Add(User user) => AddedUsers.Add(user);
        public void Update(User user) => UpdatedUsers.Add(user);
        public void Remove(int userId) => RemovedUserIds.Add(userId);
    }
}
