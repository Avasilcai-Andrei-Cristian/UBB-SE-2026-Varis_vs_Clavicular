using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Tests;

[TestFixture]
public class JobSkillRepositoryTests
{
    private JobSkillRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new JobSkillRepository();
    }

    [Test]
    public void GetById_ExistingCompositeId_ReturnsJobSkill()
    {
        var result = _repository.GetById(1, 2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.JobId, Is.EqualTo(1));
        Assert.That(result.SkillId, Is.EqualTo(2));
    }

    [Test]
    public void GetById_MissingCompositeId_ReturnsNull()
    {
        var result = _repository.GetById(9999, 9999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAll_WhenCalled_ReturnsAllJobSkills()
    {
        var result = _repository.GetAll();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(20));
    }

    [Test]
    public void GetByJobId_ExistingJobId_ReturnsOnlyMatchingJobSkills()
    {
        var result = _repository.GetByJobId(1);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(js => js.JobId == 1), Is.True);
    }

    [Test]
    public void GetByJobId_MissingJobId_ReturnsEmptyList()
    {
        var result = _repository.GetByJobId(9999);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Add_NewJobSkill_AddsJobSkillToRepository()
    {
        var newJobSkill = CreateJobSkill(1000, 1000);

        _repository.Add(newJobSkill);
        var result = _repository.GetById(1000, 1000);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SkillName, Is.EqualTo("Test Job Skill"));
    }

    [Test]
    public void Add_DuplicateCompositeId_ThrowsInvalidOperationException()
    {
        var duplicateJobSkill = CreateJobSkill(1, 2);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(duplicateJobSkill));
    }

    [Test]
    public void Update_ExistingJobSkill_UpdatesStoredJobSkill()
    {
        var updatedJobSkill = CreateJobSkill(1, 2);
        updatedJobSkill.SkillName = "Updated Job Skill Name";
        updatedJobSkill.Score = 99;

        _repository.Update(updatedJobSkill);
        var result = _repository.GetById(1, 2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SkillName, Is.EqualTo("Updated Job Skill Name"));
        Assert.That(result.Score, Is.EqualTo(99));
    }

    [Test]
    public void Update_MissingJobSkill_ThrowsKeyNotFoundException()
    {
        var missingJobSkill = CreateJobSkill(9999, 9999);

        Assert.Throws<KeyNotFoundException>(() => _repository.Update(missingJobSkill));
    }

    [Test]
    public void Remove_ExistingJobSkill_RemovesJobSkillFromRepository()
    {
        _repository.Remove(1, 2);
        var result = _repository.GetById(1, 2);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Remove_MissingJobSkill_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Remove(9999, 9999));
    }

    private static JobSkill CreateJobSkill(int jobId, int skillId)
    {
        return new JobSkill
        {
            JobId = jobId,
            SkillId = skillId,
            SkillName = "Test Job Skill",
            Score = 50
        };
    }
}
