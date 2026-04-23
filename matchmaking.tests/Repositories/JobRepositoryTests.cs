using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class JobRepositoryTests
{
    private readonly JobRepository repository = new ();

    [Fact]
    public void GetById_ExistingJobId_ReturnsJob()
    {
        var result = repository.GetById(1);

        result.Should().NotBeNull();
        result!.JobId.Should().Be(1);
    }

    [Fact]
    public void GetById_MissingJobId_ReturnsNull()
    {
        var result = repository.GetById(-1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenCalled_ReturnsAllJobs()
    {
        var result = repository.GetAll();

        result.Should().HaveCount(17);
    }

    [Fact]
    public void GetByCompanyId_ExistingCompanyId_ReturnsOnlyMatchingJobs()
    {
        var result = repository.GetByCompanyId(1);

        result.Should().NotBeEmpty();
        result.All(j => j.CompanyId == 1).Should().BeTrue();
    }

    [Fact]
    public void GetByCompanyId_MissingCompanyId_ReturnsEmptyList()
    {
        var result = repository.GetByCompanyId(9999);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Add_NewJob_AddsJobToRepository()
    {
        var newJob = CreateJob(1000);

        repository.Add(newJob);
        var result = repository.GetById(1000);

        result.Should().NotBeNull();
        result!.JobTitle.Should().Be("Test Job");
    }

    [Fact]
    public void Add_DuplicateJobId_ThrowsInvalidOperationException()
    {
        var duplicateJob = CreateJob(1);

        Action act = () => repository.Add(duplicateJob);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingJob_UpdatesStoredJob()
    {
        var updatedJob = CreateJob(1);
        updatedJob.JobTitle = "Updated Job Title";
        updatedJob.Location = "Updated Location";
        updatedJob.PromotionLevel = 5;

        repository.Update(updatedJob);
        var result = repository.GetById(1);

        result.Should().NotBeNull();
        result!.JobTitle.Should().Be("Updated Job Title");
        result.Location.Should().Be("Updated Location");
        result.PromotionLevel.Should().Be(5);
    }

    [Fact]
    public void Update_MissingJob_ThrowsKeyNotFoundException()
    {
        var missingJob = CreateJob(9999);

        Action act = () => repository.Update(missingJob);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingJob_RemovesJobFromRepository()
    {
        repository.Remove(1);
        var result = repository.GetById(1);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingJob_ThrowsKeyNotFoundException()
    {
        Action act = () => repository.Remove(9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static Job CreateJob(int jobId)
    {
        return new Job
        {
            JobId = jobId,
            JobTitle = "Test Job",
            JobDescription = "Test description",
            Location = "Cluj-Napoca",
            EmploymentType = "Full-time",
            CompanyId = 1,
            PromotionLevel = 2
        };
    }
}
