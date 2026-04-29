using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class JobRepositoryTests
{
    [Fact]
    public void Add_stores_new_job_and_get_by_id_returns_it()
    {
        var repository = new JobRepository(Enumerable.Empty<Job>());
        var backendJob = new Job
        {
            JobId = 1,
            JobTitle = "Backend Engineer",
            JobDescription = "Build REST APIs",
            Location = "Cluj-Napoca",
            EmploymentType = "Full-time",
            CompanyId = 10,
            PromotionLevel = 2
        };
        repository.Add(backendJob);
        var retrieved = repository.GetById(1);
        retrieved.Should().NotBeNull();
        retrieved!.JobTitle.Should().Be("Backend Engineer");
        retrieved.CompanyId.Should().Be(10);
    }

    [Fact]
    public void GetById_returns_null_when_job_id_does_not_exist()
    {
        var repository = new JobRepository(Enumerable.Empty<Job>());
        const int nonExistentJobId = 999;
        var result = repository.GetById(nonExistentJobId);
        result.Should().BeNull();
    }

    [Fact]
    public void Update_changes_title_and_location_of_existing_job()
    {
        var originalJob = new Job
        {
            JobId = 5,
            JobTitle = "Junior Dev",
            JobDescription = "Entry level",
            Location = "Bucharest",
            EmploymentType = "Full-time",
            CompanyId = 3,
            PromotionLevel = 1
        };
        var repository = new JobRepository(new[] { originalJob });
        var updatedJob = new Job
        {
            JobId = 5,
            JobTitle = "Senior Dev",
            JobDescription = "Experienced level",
            Location = "Timisoara",
            EmploymentType = "Remote",
            CompanyId = 3,
            PromotionLevel = 4
        };
        repository.Update(updatedJob);
        var retrieved = repository.GetById(5);
        retrieved!.JobTitle.Should().Be("Senior Dev");
        retrieved.Location.Should().Be("Timisoara");
        retrieved.EmploymentType.Should().Be("Remote");
        retrieved.PromotionLevel.Should().Be(4);
    }

    [Fact]
    public void Remove_eliminates_job_so_it_is_no_longer_retrievable()
    {
        var jobToRemove = new Job
        {
            JobId = 7,
            JobTitle = "QA Engineer",
            JobDescription = "Test automation",
            Location = "Iasi",
            EmploymentType = "Part-time",
            CompanyId = 2,
            PromotionLevel = 1
        };
        var repository = new JobRepository(new[] { jobToRemove });
        repository.Remove(7);
        repository.GetById(7).Should().BeNull();
    }

    [Fact]
    public void GetByCompanyId_returns_only_jobs_belonging_to_specified_company()
    {
        const int targetCompanyId = 4;
        const int otherCompanyId = 9;
        var companyFourJobOne = new Job { JobId = 1, JobTitle = "Dev A", CompanyId = targetCompanyId };
        var companyFourJobTwo = new Job { JobId = 2, JobTitle = "Dev B", CompanyId = targetCompanyId };
        var otherCompanyJob = new Job { JobId = 3, JobTitle = "Dev C", CompanyId = otherCompanyId };
        var repository = new JobRepository(new[] { companyFourJobOne, companyFourJobTwo, otherCompanyJob });
        var result = repository.GetByCompanyId(targetCompanyId);
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(job => job.CompanyId.Should().Be(targetCompanyId));
    }
}