using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class SkillRepositoryTests
{
    private readonly SkillRepository repository = new ();

    [Fact]
    public void GetById_ExistingCompositeId_ReturnsSkill()
    {
        var result = repository.GetById(1, 1);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.SkillId.Should().Be(1);
    }

    [Fact]
    public void GetById_MissingCompositeId_ReturnsNull()
    {
        var result = repository.GetById(9999, 9999);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenCalled_ReturnsAllSkills()
    {
        var result = repository.GetAll();

        result.Should().HaveCount(52);
    }

    [Fact]
    public void GetByUserId_ExistingUserId_ReturnsOnlyMatchingSkills()
    {
        var result = repository.GetByUserId(2);

        result.Should().NotBeEmpty();
        result.All(s => s.UserId == 2).Should().BeTrue();
    }

    [Fact]
    public void GetByUserId_MissingUserId_ReturnsEmptyList()
    {
        var result = repository.GetByUserId(9999);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDistinctSkillCatalog_WhenCalled_ReturnsDistinctSkillsOrderedByName()
    {
        var result = repository.GetDistinctSkillCatalog();

        result.Should().NotBeEmpty();
        result.Select(s => s.SkillId).Distinct().Count().Should().Be(result.Count);

        var orderedByName = result.OrderBy(s => s.Name).ToList();
        result.Should().Equal(orderedByName);
    }

    [Fact]
    public void Add_NewSkill_AddsSkillToRepository()
    {
        var newSkill = CreateSkill(1000, 1000);

        repository.Add(newSkill);
        var result = repository.GetById(1000, 1000);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be("Test Skill");
    }

    [Fact]
    public void Add_DuplicateCompositeId_ThrowsInvalidOperationException()
    {
        var duplicateSkill = CreateSkill(1, 1);

        Action act = () => repository.Add(duplicateSkill);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingSkill_UpdatesStoredSkill()
    {
        var updatedSkill = CreateSkill(1, 1);
        updatedSkill.SkillName = "Updated Skill Name";
        updatedSkill.Score = 99;

        repository.Update(updatedSkill);
        var result = repository.GetById(1, 1);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be("Updated Skill Name");
        result.Score.Should().Be(99);
    }

    [Fact]
    public void Update_MissingSkill_ThrowsKeyNotFoundException()
    {
        var missingSkill = CreateSkill(9999, 9999);

        Action act = () => repository.Update(missingSkill);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingSkill_RemovesSkillFromRepository()
    {
        repository.Remove(1, 1);
        var result = repository.GetById(1, 1);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingSkill_ThrowsKeyNotFoundException()
    {
        Action act = () => repository.Remove(9999, 9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static Skill CreateSkill(int userId, int skillId)
    {
        return new Skill
        {
            UserId = userId,
            SkillId = skillId,
            SkillName = "Test Skill",
            Score = 50
        };
    }
}
