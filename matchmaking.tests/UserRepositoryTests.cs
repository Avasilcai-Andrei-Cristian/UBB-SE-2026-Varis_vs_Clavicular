using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class UserRepositoryTests
{
    [Fact]
    public void GetById_AddedUserId_ReturnsUser()
    {
        var user = CreateUser(1000);
        var repository = CreateRepositoryWith(user);

        var result = repository.GetById(user.UserId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.UserId);
        result.Name.Should().Be(user.Name);
    }

    [Fact]
    public void GetById_MissingUserId_ReturnsNull()
    {
        var repository = CreateRepositoryWith();

        var result = repository.GetById(-1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenUsersAdded_ReturnsAddedUsers()
    {
        var firstUser = CreateUser(1000);
        var secondUser = CreateUser(1001);
        var repository = CreateRepositoryWith(firstUser, secondUser);

        var result = repository.GetAll();

        result.Should().HaveCount(2);
        result.Select(item => item.UserId).Should().BeEquivalentTo(new[] { firstUser.UserId, secondUser.UserId });
    }

    [Fact]
    public void Add_NewUser_AddsUserToRepository()
    {
        var repository = CreateRepositoryWith();
        var newUser = CreateUser(1000);

        repository.Add(newUser);
        var result = repository.GetById(newUser.UserId);

        result.Should().NotBeNull();
        result!.Name.Should().Be(newUser.Name);
    }

    [Fact]
    public void Add_DuplicateUserId_ThrowsInvalidOperationException()
    {
        var existingUser = CreateUser(1000);
        var repository = CreateRepositoryWith(existingUser);
        var duplicateUser = CreateUser(existingUser.UserId);

        Action act = () => repository.Add(duplicateUser);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingUser_UpdatesStoredUser()
    {
        var existingUser = CreateUser(1000);
        var repository = CreateRepositoryWith(existingUser);
        var updatedUser = CreateUser(existingUser.UserId);
        updatedUser.Name = "Updated Name";
        updatedUser.Location = "Updated City";

        repository.Update(updatedUser);
        var result = repository.GetById(updatedUser.UserId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Location.Should().Be("Updated City");
    }

    [Fact]
    public void Update_MissingUser_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();
        var missingUser = CreateUser(9999);

        Action act = () => repository.Update(missingUser);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingUser_RemovesUserFromRepository()
    {
        var user = CreateUser(1000);
        var repository = CreateRepositoryWith(user);

        repository.Remove(user.UserId);
        var result = repository.GetById(user.UserId);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingUser_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();

        Action act = () => repository.Remove(9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static UserRepository CreateRepositoryWith(params User[] users)
    {
        var repository = new UserRepository([]);
        foreach (var user in users)
        {
            repository.Add(user);
        }

        return repository;
    }

    private static User CreateUser(int userId)
    {
        return new User
        {
            UserId = userId,
            Name = "Test User",
            Location = "Cluj-Napoca",
            Email = "test.user@mail.com",
            Phone = "0700123456",
            YearsOfExperience = 3,
            Education = "BSc Computer Science",
            Resume = "Test resume",
            PreferredEmploymentType = "Full-time"
        };
    }
}
