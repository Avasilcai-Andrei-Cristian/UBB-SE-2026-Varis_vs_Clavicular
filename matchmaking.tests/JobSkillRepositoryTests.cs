using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class JobSkillRepositoryTests
{
    [Fact]
    public void GetById_AddedCompositeId_ReturnsJobSkill()
    {
        var jobSkill = CreateJobSkill(1000, 1000);
        var repository = CreateRepositoryWith(jobSkill);

        var result = repository.GetById(jobSkill.JobId, jobSkill.SkillId);

        result.Should().NotBeNull();
        result!.JobId.Should().Be(jobSkill.JobId);
        result.SkillId.Should().Be(jobSkill.SkillId);
        result.SkillName.Should().Be(jobSkill.SkillName);
    }

    [Fact]
    public void GetById_MissingCompositeId_ReturnsNull()
    {
        var repository = CreateRepositoryWith();

        var result = repository.GetById(9999, 9999);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenJobSkillsAdded_ReturnsAddedJobSkills()
    {
        var firstJobSkill = CreateJobSkill(1000, 1000);
        var secondJobSkill = CreateJobSkill(1001, 1001);
        var repository = CreateRepositoryWith(firstJobSkill, secondJobSkill);

        var result = repository.GetAll();

        result.Should().HaveCount(2);
        result.Select(item => (item.JobId, item.SkillId)).Should().BeEquivalentTo(new[] { (firstJobSkill.JobId, firstJobSkill.SkillId), (secondJobSkill.JobId, secondJobSkill.SkillId) });
    }

    [Fact]
    public void GetByJobId_WhenMatchingJobSkillAdded_ReturnsOnlyMatchingJobSkills()
    {
        var matchingJobId = 1000;
        var matchingJobSkill = CreateJobSkill(matchingJobId, 1000);
        var otherJobSkill = CreateJobSkill(jobId: 1001, skillId: 1001);
        var repository = CreateRepositoryWith(matchingJobSkill, otherJobSkill);

        var result = repository.GetByJobId(matchingJobId);

        result.Should().ContainSingle(item => item.SkillId == matchingJobSkill.SkillId);
        result.All(item => item.JobId == matchingJobId).Should().BeTrue();
    }

    [Fact]
    public void GetByJobId_MissingJobId_ReturnsEmptyList()
    {
        var repository = CreateRepositoryWith(CreateJobSkill(1000, 1000));

        var result = repository.GetByJobId(9999);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Add_NewJobSkill_AddsJobSkillToRepository()
    {
        var repository = CreateRepositoryWith();
        var newJobSkill = CreateJobSkill(1000, 1000);

        repository.Add(newJobSkill);
        var result = repository.GetById(newJobSkill.JobId, newJobSkill.SkillId);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be(newJobSkill.SkillName);
    }

    [Fact]
    public void Add_DuplicateCompositeId_ThrowsInvalidOperationException()
    {
        var existingJobSkill = CreateJobSkill(1000, 1000);
        var repository = CreateRepositoryWith(existingJobSkill);
        var duplicateJobSkill = CreateJobSkill(existingJobSkill.JobId, existingJobSkill.SkillId);

        Action act = () => repository.Add(duplicateJobSkill);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingJobSkill_UpdatesStoredJobSkill()
    {
        var existingJobSkill = CreateJobSkill(1000, 1000);
        var repository = CreateRepositoryWith(existingJobSkill);
        var updatedJobSkill = CreateJobSkill(existingJobSkill.JobId, existingJobSkill.SkillId);
        updatedJobSkill.SkillName = "Updated Job Skill Name";
        updatedJobSkill.Score = 99;

        repository.Update(updatedJobSkill);
        var result = repository.GetById(updatedJobSkill.JobId, updatedJobSkill.SkillId);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be("Updated Job Skill Name");
        result.Score.Should().Be(99);
    }

    [Fact]
    public void Update_MissingJobSkill_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();
        var missingJobSkill = CreateJobSkill(9999, 9999);

        Action act = () => repository.Update(missingJobSkill);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingJobSkill_RemovesJobSkillFromRepository()
    {
        var jobSkill = CreateJobSkill(1000, 1000);
        var repository = CreateRepositoryWith(jobSkill);

        repository.Remove(jobSkill.JobId, jobSkill.SkillId);
        var result = repository.GetById(jobSkill.JobId, jobSkill.SkillId);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingJobSkill_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();

        Action act = () => repository.Remove(9999, 9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static JobSkillRepository CreateRepositoryWith(params JobSkill[] jobSkills)
    {
        var repository = new JobSkillRepository([]);
        foreach (var jobSkill in jobSkills)
        {
            repository.Add(jobSkill);
        }

        return repository;
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
