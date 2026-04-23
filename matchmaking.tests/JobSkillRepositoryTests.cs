using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class JobSkillRepositoryTests
{
    private readonly JobSkillRepository repository = new ();

    [Fact]
    public void GetById_ExistingCompositeId_ReturnsJobSkill()
    {
        var result = repository.GetById(1, 2);

        result.Should().NotBeNull();
        result!.JobId.Should().Be(1);
        result.SkillId.Should().Be(2);
    }

    [Fact]
    public void GetById_MissingCompositeId_ReturnsNull()
    {
        var result = repository.GetById(9999, 9999);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenCalled_ReturnsAllJobSkills()
    {
        var result = repository.GetAll();

        result.Should().HaveCount(20);
    }

    [Fact]
    public void GetByJobId_ExistingJobId_ReturnsOnlyMatchingJobSkills()
    {
        var result = repository.GetByJobId(1);

        result.Should().NotBeEmpty();
        result.All(js => js.JobId == 1).Should().BeTrue();
    }

    [Fact]
    public void GetByJobId_MissingJobId_ReturnsEmptyList()
    {
        var result = repository.GetByJobId(9999);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Add_NewJobSkill_AddsJobSkillToRepository()
    {
        var newJobSkill = CreateJobSkill(1000, 1000);

        repository.Add(newJobSkill);
        var result = repository.GetById(1000, 1000);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be("Test Job Skill");
    }

    [Fact]
    public void Add_DuplicateCompositeId_ThrowsInvalidOperationException()
    {
        var duplicateJobSkill = CreateJobSkill(1, 2);

        Action act = () => repository.Add(duplicateJobSkill);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingJobSkill_UpdatesStoredJobSkill()
    {
        var updatedJobSkill = CreateJobSkill(1, 2);
        updatedJobSkill.SkillName = "Updated Job Skill Name";
        updatedJobSkill.Score = 99;

        repository.Update(updatedJobSkill);
        var result = repository.GetById(1, 2);

        result.Should().NotBeNull();
        result!.SkillName.Should().Be("Updated Job Skill Name");
        result.Score.Should().Be(99);
    }

    [Fact]
    public void Update_MissingJobSkill_ThrowsKeyNotFoundException()
    {
        var missingJobSkill = CreateJobSkill(9999, 9999);

        Action act = () => repository.Update(missingJobSkill);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingJobSkill_RemovesJobSkillFromRepository()
    {
        repository.Remove(1, 2);
        var result = repository.GetById(1, 2);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingJobSkill_ThrowsKeyNotFoundException()
    {
        Action act = () => repository.Remove(9999, 9999);

        act.Should().Throw<KeyNotFoundException>();
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
