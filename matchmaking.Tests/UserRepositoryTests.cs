using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class UserRepositoryTests
{
    [Fact]
    public void GetById_returns_user_when_user_id_exists()
    {
        const int existingUserId = 5;
        var existingUser = new User
        {
            UserId = existingUserId,
            Name = "Elena Matei",
            Location = "Brasov",
            PreferredLocation = "Brasov",
            Email = "elena.matei@mail.com",
            Phone = "0700000005",
            YearsOfExperience = 3,
            Education = "BSc Mathematics and CS",
            Resume = "Data analyst",
            PreferredEmploymentType = "Hybrid"
        };
        var repository = new UserRepository(new[] { existingUser });
        var result = repository.GetById(existingUserId);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Elena Matei");
        result.Email.Should().Be("elena.matei@mail.com");
    }

    [Fact]
    public void Update_replaces_all_mutable_fields_on_existing_user()
    {
        const int userId = 8;
        var originalUser = new User
        {
            UserId = userId,
            Name = "Horia Vasile",
            Location = "Constanta",
            PreferredLocation = "Constanta",
            Email = "horia.vasile@mail.com",
            Phone = "0700000008",
            YearsOfExperience = 7,
            Education = "BSc Information Systems",
            Resume = "Tech lead",
            PreferredEmploymentType = "Hybrid"
        };
        var repository = new UserRepository(new[] { originalUser });
        var updatedUser = new User
        {
            UserId = userId,
            Name = "Horia Vasile Updated",
            Location = "Cluj-Napoca",
            PreferredLocation = "Cluj-Napoca",
            Email = "horia.updated@mail.com",
            Phone = "0700000099",
            YearsOfExperience = 10,
            Education = "MSc Computer Science",
            Resume = "Principal Engineer",
            PreferredEmploymentType = "Remote"
        };
        repository.Update(updatedUser);
        var retrieved = repository.GetById(userId);
        retrieved!.Name.Should().Be("Horia Vasile Updated");
        retrieved.Location.Should().Be("Cluj-Napoca");
        retrieved.Email.Should().Be("horia.updated@mail.com");
        retrieved.YearsOfExperience.Should().Be(10);
        retrieved.PreferredEmploymentType.Should().Be("Remote");
    }
}