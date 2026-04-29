using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Models;
using matchmaking.Services;
namespace matchmaking.Tests;
public sealed class UserStatusServiceTests
{
    private const int UserId = 1;
    private const int JobId = 100;
    private const int CompanyId = 50;
    [Fact]
    public void GetApplicationsForUser_maps_company_name_and_status()
    {
        var matchRepository = new FakeUserStatusMatchRepository
        {
            MatchesByUserId =
            {
                [UserId] =
                [
                    new Match
                    {
                        MatchId = 10,
                        UserId = UserId,
                        JobId = JobId,
                        Status = MatchStatus.Accepted,
                        Timestamp = new DateTime(2026, 1, 1),
                        FeedbackMessage = "Good"
                    }
                ]
            }
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = JobId, CompanyId = CompanyId, JobDescription = "Role" }]
        };
        var companyService = new FakeCompanyService
        {
            Companies = [new Company { CompanyId = CompanyId, CompanyName = "Acme" }]
        };
        var service = new UserStatusService(matchRepository, jobService, companyService, new FakeSkillService(), new FakeJobSkillService());
        var applications = service.GetApplicationsForUser(UserId);
        applications.Should().ContainSingle();
        applications[0].CompanyName.Should().Be("Acme");
        applications[0].Status.Should().Be(MatchStatus.Accepted);
        applications[0].FeedbackMessage.Should().Be("Good");
    }
    [Fact]
    public void GetApplicationsForUser_skips_missing_jobs_and_uses_unknown_company()
    {
        var matchRepository = new FakeUserStatusMatchRepository
        {
            MatchesByUserId =
            {
                [UserId] =
                [
                    new Match { MatchId = 10, UserId = UserId, JobId = JobId, Status = MatchStatus.Applied },
                    new Match { MatchId = 11, UserId = UserId, JobId = 200, Status = MatchStatus.Applied }
                ]
            }
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = JobId, CompanyId = CompanyId, JobDescription = "Role" }]
        };
        var companyService = new FakeCompanyService();
        var service = new UserStatusService(matchRepository, jobService, companyService, new FakeSkillService(), new FakeJobSkillService());
        var applications = service.GetApplicationsForUser(UserId);
        applications.Should().ContainSingle();
        applications[0].CompanyName.Should().Be("Unknown Company");
    }
    [Fact]
    public void GetApplicationsForUser_calculates_compatibility_score()
    {
        var matchRepository = new FakeUserStatusMatchRepository
        {
            MatchesByUserId =
            {
                [UserId] =
                [
                    new Match { MatchId = 10, UserId = UserId, JobId = JobId, Status = MatchStatus.Applied }
                ]
            }
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = JobId, CompanyId = CompanyId, JobDescription = "Role" }]
        };
        var skillService = new FakeSkillService
        {
            SkillsByUserId =
            {
                [UserId] =
                [
                    new Skill { SkillId = 1, Score = 80 },
                    new Skill { SkillId = 2, Score = 50 }
                ]
            }
        };
        var jobSkillService = new FakeJobSkillService
        {
            SkillsByJobId =
            {
                [JobId] =
                [
                    new JobSkill { JobId = JobId, SkillId = 1, Score = 100 },
                    new JobSkill { JobId = JobId, SkillId = 2, Score = 50 }
                ]
            }
        };
        var service = new UserStatusService(matchRepository, jobService, new FakeCompanyService(), skillService, jobSkillService);
        var applications = service.GetApplicationsForUser(UserId);
        applications.Should().ContainSingle();
        applications[0].CompatibilityScore.Should().Be(90);
    }
    [Fact]
    public void GetApplicationsForUser_returns_full_score_when_job_requires_no_skills()
    {
        var matchRepository = new FakeUserStatusMatchRepository
        {
            MatchesByUserId =
            {
                [UserId] =
                [
                    new Match { MatchId = 10, UserId = UserId, JobId = JobId, Status = MatchStatus.Applied }
                ]
            }
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = JobId, CompanyId = CompanyId, JobDescription = "Role" }]
        };
        var service = new UserStatusService(matchRepository, jobService, new FakeCompanyService(), new FakeSkillService(), new FakeJobSkillService());
        var applications = service.GetApplicationsForUser(UserId);
        applications.Should().ContainSingle();
        applications[0].CompatibilityScore.Should().Be(100);
    }
    private sealed class FakeUserStatusMatchRepository : IUserStatusMatchRepository
    {
        public Dictionary<int, List<Match>> MatchesByUserId { get; set; } = new();
        public IReadOnlyList<Match> GetRejectedByUserId(int userId) => MatchesByUserId.TryGetValue(userId, out var list) ? list : [];
        public IReadOnlyList<Match> GetByUserId(int userId) => MatchesByUserId.TryGetValue(userId, out var list) ? list : [];
    }
    private sealed class FakeJobService : IJobService
    {
        public List<Job> Jobs { get; set; } = [];
        public Job? GetById(int jobId) => Jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => Jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => Jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) { }
        public void Update(Job job) { }
        public void Remove(int jobId) { }
    }
    private sealed class FakeCompanyService : ICompanyService
    {
        public List<Company> Companies { get; set; } = [];
        public Company? GetById(int companyId) => Companies.FirstOrDefault(company => company.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => Companies;
        public void Add(Company company) { }
        public void Update(Company company) { }
        public void Remove(int companyId) { }
    }
    private sealed class FakeSkillService : ISkillService
    {
        public Dictionary<int, List<Skill>> SkillsByUserId { get; set; } = new();
        public IReadOnlyList<Skill> GetByUserId(int userId) => SkillsByUserId.TryGetValue(userId, out var list) ? list : [];
        public Skill? GetById(int userId, int skillId) => null;
        public IReadOnlyList<Skill> GetAll() => SkillsByUserId.SelectMany(pair => pair.Value).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => [];
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }
    private sealed class FakeJobSkillService : IJobSkillService
    {
        public Dictionary<int, List<JobSkill>> SkillsByJobId { get; set; } = new();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => SkillsByJobId.TryGetValue(jobId, out var list) ? list : [];
        public JobSkill? GetById(int jobId, int skillId) => null;
        public IReadOnlyList<JobSkill> GetAll() => SkillsByJobId.SelectMany(pair => pair.Value).ToList();
        public void Add(JobSkill jobSkill) { }
        public void Update(JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) { }
    }
}
