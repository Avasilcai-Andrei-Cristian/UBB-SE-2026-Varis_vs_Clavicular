using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public void Add_delegates_user_to_user_repository()
    {
        var userRepository = new FakeUserRepository();
        var service = new UserService(userRepository);
        var newUser = new User
        {
            UserId = 42,
            Name = "Test User",
            Email = "test@example.com",
            Location = "Cluj-Napoca",
            PreferredLocation = "Cluj-Napoca",
            PreferredEmploymentType = "Full-time"
        };
        service.Add(newUser);
        userRepository.AddedUsers.Should().ContainSingle();
        userRepository.AddedUsers[0].UserId.Should().Be(42);
        userRepository.AddedUsers[0].Name.Should().Be("Test User");
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> AddedUsers { get; } = [];
        public User? GetById(int userId) => AddedUsers.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => AddedUsers;
        public void Add(User user) => AddedUsers.Add(user);
        public void Update(User user) { }
        public void Remove(int userId) => AddedUsers.RemoveAll(user => user.UserId == userId);
    }
}