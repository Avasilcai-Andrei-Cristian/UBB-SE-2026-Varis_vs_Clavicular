using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class JobRepositoryTests
{
    [Fact]
    public void GetById_AddedJobId_ReturnsJob()
    {
        var job = CreateJob(1000);
        var repository = CreateRepositoryWith(job);

        var result = repository.GetById(job.JobId);

        result.Should().NotBeNull();
        result!.JobId.Should().Be(job.JobId);
        result.JobTitle.Should().Be(job.JobTitle);
    }

    [Fact]
    public void GetById_MissingJobId_ReturnsNull()
    {
        var repository = CreateRepositoryWith();

        var result = repository.GetById(-1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenJobsAdded_ReturnsAddedJobs()
    {
        var firstJob = CreateJob(1000, companyId: 2000);
        var secondJob = CreateJob(1001, companyId: 2001);
        var repository = CreateRepositoryWith(firstJob, secondJob);

        var result = repository.GetAll();

        result.Should().HaveCount(2);
        result.Select(item => item.JobId).Should().BeEquivalentTo(new[] { firstJob.JobId, secondJob.JobId });
    }

    [Fact]
    public void GetByCompanyId_WhenMatchingJobAdded_ReturnsOnlyMatchingJobs()
    {
        var matchingCompanyId = 2000;
        var matchingJob = CreateJob(1000, matchingCompanyId);
        var otherJob = CreateJob(1001, companyId: 2001);
        var repository = CreateRepositoryWith(matchingJob, otherJob);

        var result = repository.GetByCompanyId(matchingCompanyId);

        result.Should().ContainSingle(item => item.JobId == matchingJob.JobId);
        result.All(item => item.CompanyId == matchingCompanyId).Should().BeTrue();
    }

    [Fact]
    public void GetByCompanyId_MissingCompanyId_ReturnsEmptyList()
    {
        var repository = CreateRepositoryWith(CreateJob(1000, companyId: 2000));

        var result = repository.GetByCompanyId(9999);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Add_NewJob_AddsJobToRepository()
    {
        var repository = CreateRepositoryWith();
        var newJob = CreateJob(1000);

        repository.Add(newJob);
        var result = repository.GetById(newJob.JobId);

        result.Should().NotBeNull();
        result!.JobTitle.Should().Be(newJob.JobTitle);
    }

    [Fact]
    public void Add_DuplicateJobId_ThrowsInvalidOperationException()
    {
        var existingJob = CreateJob(1000);
        var repository = CreateRepositoryWith(existingJob);
        var duplicateJob = CreateJob(existingJob.JobId);

        Action act = () => repository.Add(duplicateJob);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingJob_UpdatesStoredJob()
    {
        var existingJob = CreateJob(1000);
        var repository = CreateRepositoryWith(existingJob);
        var updatedJob = CreateJob(existingJob.JobId);
        updatedJob.JobTitle = "Updated Job Title";
        updatedJob.Location = "Updated Location";
        updatedJob.PromotionLevel = 5;

        repository.Update(updatedJob);
        var result = repository.GetById(updatedJob.JobId);

        result.Should().NotBeNull();
        result!.JobTitle.Should().Be("Updated Job Title");
        result.Location.Should().Be("Updated Location");
        result.PromotionLevel.Should().Be(5);
    }

    [Fact]
    public void Update_MissingJob_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();
        var missingJob = CreateJob(9999);

        Action act = () => repository.Update(missingJob);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingJob_RemovesJobFromRepository()
    {
        var job = CreateJob(1000);
        var repository = CreateRepositoryWith(job);

        repository.Remove(job.JobId);
        var result = repository.GetById(job.JobId);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingJob_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();

        Action act = () => repository.Remove(9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static JobRepository CreateRepositoryWith(params Job[] jobs)
    {
        var repository = new JobRepository([]);
        foreach (var job in jobs)
        {
            repository.Add(job);
        }

        return repository;
    }

    private static Job CreateJob(int jobId, int companyId = 1)
    {
        return new Job
        {
            JobId = jobId,
            JobTitle = "Test Job",
            JobDescription = "Test description",
            Location = "Cluj-Napoca",
            EmploymentType = "Full-time",
            CompanyId = companyId,
            PromotionLevel = 2
        };
    }
}
