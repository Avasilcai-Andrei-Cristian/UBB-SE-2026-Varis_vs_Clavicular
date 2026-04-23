using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Tests;

[TestFixture]
public class UserRepositoryTests
{
    private UserRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new UserRepository();
    }

    [Test]
    public void GetById_ExistingUserId_ReturnsUser()
    {
        var result = _repository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserId, Is.EqualTo(1));
    }

    [Test]
    public void GetById_MissingUserId_ReturnsNull()
    {
        var result = _repository.GetById(-1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAll_WhenCalled_ReturnsAllUsers()
    {
        var result = _repository.GetAll();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(20));
    }

    [Test]
    public void Add_NewUser_AddsUserToRepository()
    {
        var newUser = CreateUser(1000);

        _repository.Add(newUser);
        var result = _repository.GetById(1000);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test User"));
    }

    [Test]
    public void Add_DuplicateUserId_ThrowsInvalidOperationException()
    {
        var duplicateUser = CreateUser(1);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(duplicateUser));
    }

    [Test]
    public void Update_ExistingUser_UpdatesStoredUser()
    {
        var updatedUser = CreateUser(1);
        updatedUser.Name = "Updated Name";
        updatedUser.Location = "Updated City";

        _repository.Update(updatedUser);
        var result = _repository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
        Assert.That(result.Location, Is.EqualTo("Updated City"));
    }

    [Test]
    public void Update_MissingUser_ThrowsKeyNotFoundException()
    {
        var missingUser = CreateUser(9999);

        Assert.Throws<KeyNotFoundException>(() => _repository.Update(missingUser));
    }

    [Test]
    public void Remove_ExistingUser_RemovesUserFromRepository()
    {
        _repository.Remove(1);
        var result = _repository.GetById(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Remove_MissingUser_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Remove(9999));
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