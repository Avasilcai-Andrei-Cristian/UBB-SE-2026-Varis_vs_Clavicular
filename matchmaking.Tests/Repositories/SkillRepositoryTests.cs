using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Tests;

[TestFixture]
public class SkillRepositoryTests
{
    private SkillRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new SkillRepository();
    }

    [Test]
    public void GetById_ExistingCompositeId_ReturnsSkill()
    {
        var result = _repository.GetById(1, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserId, Is.EqualTo(1));
        Assert.That(result.SkillId, Is.EqualTo(1));
    }

    [Test]
    public void GetById_MissingCompositeId_ReturnsNull()
    {
        var result = _repository.GetById(9999, 9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAll_WhenCalled_ReturnsAllSkills()
    {
        var result = _repository.GetAll();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(52));
    }

    [Test]
    public void GetByUserId_ExistingUserId_ReturnsOnlyMatchingSkills()
    {
        var result = _repository.GetByUserId(2);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(s => s.UserId == 2), Is.True);
    }

    [Test]
    public void GetByUserId_MissingUserId_ReturnsEmptyList()
    {
        var result = _repository.GetByUserId(9999);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDistinctSkillCatalog_WhenCalled_ReturnsDistinctSkillsOrderedByName()
    {
        var result = _repository.GetDistinctSkillCatalog();

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Select(s => s.SkillId).Distinct().Count(), Is.EqualTo(result.Count));

        var orderedByName = result.OrderBy(s => s.Name).ToList();
        Assert.That(result, Is.EqualTo(orderedByName));
    }

    [Test]
    public void Add_NewSkill_AddsSkillToRepository()
    {
        var newSkill = CreateSkill(1000, 1000);

        _repository.Add(newSkill);
        var result = _repository.GetById(1000, 1000);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SkillName, Is.EqualTo("Test Skill"));
    }

    [Test]
    public void Add_DuplicateCompositeId_ThrowsInvalidOperationException()
    {
        var duplicateSkill = CreateSkill(1, 1);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(duplicateSkill));
    }

    [Test]
    public void Update_ExistingSkill_UpdatesStoredSkill()
    {
        var updatedSkill = CreateSkill(1, 1);
        updatedSkill.SkillName = "Updated Skill Name";
        updatedSkill.Score = 99;

        _repository.Update(updatedSkill);
        var result = _repository.GetById(1, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SkillName, Is.EqualTo("Updated Skill Name"));
        Assert.That(result.Score, Is.EqualTo(99));
    }

    [Test]
    public void Update_MissingSkill_ThrowsKeyNotFoundException()
    {
        var missingSkill = CreateSkill(9999, 9999);

        Assert.Throws<KeyNotFoundException>(() => _repository.Update(missingSkill));
    }

    [Test]
    public void Remove_ExistingSkill_RemovesSkillFromRepository()
    {
        _repository.Remove(1, 1);
        var result = _repository.GetById(1, 1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Remove_MissingSkill_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Remove(9999, 9999));
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
