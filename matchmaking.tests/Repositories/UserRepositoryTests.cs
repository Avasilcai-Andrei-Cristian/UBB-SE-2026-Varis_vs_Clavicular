using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class UserRepositoryTests
{
    private readonly UserRepository repository = new ();

    [Fact]
    public void GetById_ExistingUserId_ReturnsUser()
    {
        var result = repository.GetById(1);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
    }

    [Fact]
    public void GetById_MissingUserId_ReturnsNull()
    {
        var result = repository.GetById(-1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenCalled_ReturnsAllUsers()
    {
        var result = repository.GetAll();

        result.Should().HaveCount(20);
    }

    [Fact]
    public void Add_NewUser_AddsUserToRepository()
    {
        var newUser = CreateUser(1000);

        repository.Add(newUser);
        var result = repository.GetById(1000);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test User");
    }

    [Fact]
    public void Add_DuplicateUserId_ThrowsInvalidOperationException()
    {
        var duplicateUser = CreateUser(1);

        Action act = () => repository.Add(duplicateUser);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingUser_UpdatesStoredUser()
    {
        var updatedUser = CreateUser(1);
        updatedUser.Name = "Updated Name";
        updatedUser.Location = "Updated City";

        repository.Update(updatedUser);
        var result = repository.GetById(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Location.Should().Be("Updated City");
    }

    [Fact]
    public void Update_MissingUser_ThrowsKeyNotFoundException()
    {
        var missingUser = CreateUser(9999);

        Action act = () => repository.Update(missingUser);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingUser_RemovesUserFromRepository()
    {
        repository.Remove(1);
        var result = repository.GetById(1);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingUser_ThrowsKeyNotFoundException()
    {
        Action act = () => repository.Remove(9999);

        act.Should().Throw<KeyNotFoundException>();
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
