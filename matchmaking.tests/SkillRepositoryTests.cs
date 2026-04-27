using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class SkillRepositoryTests
{
    [Fact]
    public void GetById_AddedCompositeId_ReturnsSkill()
    {
        var skill = CreateSkill(1000, 1000);
        var repository = CreateRepositoryWith(skill);

        var result = repository.GetById(skill.UserId, skill.SkillId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(skill.UserId);
        result.SkillId.Should().Be(skill.SkillId);
        result.SkillName.Should().Be(skill.SkillName);
    }

    [Fact]
    public void GetById_MissingCompositeId_ReturnsNull()
    {
        var repository = CreateRepositoryWith();

        var result = repository.GetById(9999, 9999);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenSkillsAdded_ReturnsAddedSkills()
    {
        var firstSkill = CreateSkill(1000, 1000);
        var secondSkill = CreateSkill(1001, 1001);
        var repository = CreateRepositoryWith(firstSkill, secondSkill);

        var result = repository.GetAll();

        result.Should().HaveCount(2);
        result.Select(item => (item.UserId, item.SkillId)).Should().BeEquivalentTo(new[] { (firstSkill.UserId, firstSkill.SkillId), (secondSkill.UserId, secondSkill.SkillId) });
    }

    [Fact]
    public void GetByUserId_WhenMatchingSkillAdded_ReturnsOnlyMatchingSkills()
    {
        var matchingUserId = 1000;
        var matchingSkill = CreateSkill(matchingUserId, 1000);
        var otherSkill = CreateSkill(userId: 1001, skillId: 1001);
        var repository = CreateRepositoryWith(matchingSkill, otherSkill);

        var result = repository.GetByUserId(matchingUserId);

        result.Should().ContainSingle(item => item.SkillId == matchingSkill.SkillId);
        result.All(item => item.UserId == matchingUserId).Should().BeTrue();
    }

    [Fact]
    public void GetByUserId_MissingUserId_ReturnsEmptyList()
    {
        var repository = CreateRepositoryWith(CreateSkill(1000, 1000));

        var result = repository.GetByUserId(9999);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDistinctSkillCatalog_WhenCalled_ReturnsDistinctSkillsOrderedByName()
    {
        var repository = CreateRepositoryWith(
            CreateSkill(userId: 1000, skillId: 1000, skillName: "Zeta Test Skill"),
            CreateSkill(userId: 1001, skillId: 1001, skillName: "Alpha Test Skill"),
            CreateSkill(userId: 1002, skillId: 1000, skillName: "Zeta Test Skill"));

        var result = repository.GetDistinctSkillCatalog();

        result.Should().HaveCount(2);
        result.Select(s => s.SkillId).Distinct().Count().Should().Be(result.Count);
        result.Should().Contain(item => item.SkillId == 1000 && item.Name == "Zeta Test Skill");
        result.Should().Contain(item => item.SkillId == 1001 && item.Name == "Alpha Test Skill");

        var orderedByName = result.OrderBy(s => s.Name).ToList();
        result.Should().Equal(orderedByName);
    }

    [Fact]
    public void Add_NewSkill_AddsSkillToRepository()
    {
        var repository = CreateRepositoryWith();
        var newSkill = CreateSkill(1000, 1000);

        repository.Add(newSkill);
        var result = repository.GetById(newSkill.UserId, newSkill.SkillId);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be(newSkill.SkillName);
    }

    [Fact]
    public void Add_DuplicateCompositeId_ThrowsInvalidOperationException()
    {
        var existingSkill = CreateSkill(1000, 1000);
        var repository = CreateRepositoryWith(existingSkill);
        var duplicateSkill = CreateSkill(existingSkill.UserId, existingSkill.SkillId);

        Action act = () => repository.Add(duplicateSkill);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingSkill_UpdatesStoredSkill()
    {
        var existingSkill = CreateSkill(1000, 1000);
        var repository = CreateRepositoryWith(existingSkill);
        var updatedSkill = CreateSkill(existingSkill.UserId, existingSkill.SkillId);
        updatedSkill.SkillName = "Updated Skill Name";
        updatedSkill.Score = 99;

        repository.Update(updatedSkill);
        var result = repository.GetById(updatedSkill.UserId, updatedSkill.SkillId);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be("Updated Skill Name");
        result.Score.Should().Be(99);
    }

    [Fact]
    public void Update_MissingSkill_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();
        var missingSkill = CreateSkill(9999, 9999);

        Action act = () => repository.Update(missingSkill);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingSkill_RemovesSkillFromRepository()
    {
        var skill = CreateSkill(1000, 1000);
        var repository = CreateRepositoryWith(skill);

        repository.Remove(skill.UserId, skill.SkillId);
        var result = repository.GetById(skill.UserId, skill.SkillId);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingSkill_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();

        Action act = () => repository.Remove(9999, 9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static SkillRepository CreateRepositoryWith(params Skill[] skills)
    {
        var repository = new SkillRepository([]);
        foreach (var skill in skills)
        {
            repository.Add(skill);
        }

        return repository;
    }

    private static Skill CreateSkill(int userId, int skillId, string skillName = "Test Skill")
    {
        return new Skill
        {
            UserId = userId,
            SkillId = skillId,
            SkillName = skillName,
            Score = 50
        };
    }
}
