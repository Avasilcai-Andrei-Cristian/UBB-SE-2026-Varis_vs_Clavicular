using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Tests;

[TestFixture]
public class JobRepositoryTests
{
    private JobRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new JobRepository();
    }

    [Test]
    public void GetById_ExistingJobId_ReturnsJob()
    {
        var result = _repository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.JobId, Is.EqualTo(1));
    }

    [Test]
    public void GetById_MissingJobId_ReturnsNull()
    {
        var result = _repository.GetById(-1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAll_WhenCalled_ReturnsAllJobs()
    {
        var result = _repository.GetAll();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(17));
    }

    [Test]
    public void GetByCompanyId_ExistingCompanyId_ReturnsOnlyMatchingJobs()
    {
        var result = _repository.GetByCompanyId(1);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(j => j.CompanyId == 1), Is.True);
    }

    [Test]
    public void GetByCompanyId_MissingCompanyId_ReturnsEmptyList()
    {
        var result = _repository.GetByCompanyId(9999);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Add_NewJob_AddsJobToRepository()
    {
        var newJob = CreateJob(1000);

        _repository.Add(newJob);
        var result = _repository.GetById(1000);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.JobTitle, Is.EqualTo("Test Job"));
    }

    [Test]
    public void Add_DuplicateJobId_ThrowsInvalidOperationException()
    {
        var duplicateJob = CreateJob(1);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(duplicateJob));
    }

    [Test]
    public void Update_ExistingJob_UpdatesStoredJob()
    {
        var updatedJob = CreateJob(1);
        updatedJob.JobTitle = "Updated Job Title";
        updatedJob.Location = "Updated Location";
        updatedJob.PromotionLevel = 5;

        _repository.Update(updatedJob);
        var result = _repository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.JobTitle, Is.EqualTo("Updated Job Title"));
        Assert.That(result.Location, Is.EqualTo("Updated Location"));
        Assert.That(result.PromotionLevel, Is.EqualTo(5));
    }

    [Test]
    public void Update_MissingJob_ThrowsKeyNotFoundException()
    {
        var missingJob = CreateJob(9999);

        Assert.Throws<KeyNotFoundException>(() => _repository.Update(missingJob));
    }

    [Test]
    public void Remove_ExistingJob_RemovesJobFromRepository()
    {
        _repository.Remove(1);
        var result = _repository.GetById(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Remove_MissingJob_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Remove(9999));
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
