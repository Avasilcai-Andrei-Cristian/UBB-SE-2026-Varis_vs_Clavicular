using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Models;

namespace matchmaking.Tests;

public sealed class UserStatusAndSkillGapTests
{
    [Fact]
    public void GetApplicationsForUser_WhenMatchExists_ReturnsApplicationCards()
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "good fit");

        var userStatusService = CreateUserStatusService(new[] { user }, new[] { company }, new[] { job }, new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) }, new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) }, new[] { match });

        var applications = userStatusService.GetApplicationsForUser(user.UserId);

        applications.Should().ContainSingle();
        applications[0].CompanyName.Should().Be(company.CompanyName);
        applications[0].Status.Should().Be(MatchStatus.Accepted);
    }

    [Fact]
    public void GetMissingSkills_WhenRejectedMatchesExist_ReturnsGroupedMissingSkills()
    {
        var user = TestDataFactory.CreateUser();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, 100, MatchStatus.Rejected, "missing skills");
        var service = CreateSkillGapService(new[] { user }, new[] { rejectedMatch }, new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) }, new[] { TestDataFactory.CreateJobSkill(100, 2, "SQL", 70) });

        var missingSkills = service.GetMissingSkills(user.UserId);

        missingSkills.Should().ContainSingle();
        missingSkills[0].SkillName.Should().Be("SQL");
        missingSkills[0].RejectedJobCount.Should().Be(1);
    }

    [Fact]
    public void GetUnderscoredSkills_WhenUserHasLowerScore_ReturnsImprovementItems()
    {
        var user = TestDataFactory.CreateUser();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, 100, MatchStatus.Rejected, "improve this");
        var service = CreateSkillGapService(new[] { user }, new[] { rejectedMatch }, new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 40) }, new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 70) });

        var underscoredSkills = service.GetUnderscoredSkills(user.UserId);

        underscoredSkills.Should().ContainSingle();
        underscoredSkills[0].SkillName.Should().Be("C#");
        underscoredSkills[0].AverageRequiredScore.Should().Be(70);
    }

    [Fact]
    public void GetSummary_WhenNoRejections_ReturnsNoRejectionsState()
    {
        var user = TestDataFactory.CreateUser();
        var service = CreateSkillGapService(new[] { user }, Array.Empty<Match>(), Array.Empty<Skill>(), Array.Empty<JobSkill>());

        var summary = service.GetSummary(user.UserId);

        summary.HasRejections.Should().BeFalse();
        summary.HasSkillGaps.Should().BeFalse();
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_WhenVisibleMatchesExist_ReturnsOrderedApplicants()
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "approved");

        var service = CreateCompanyStatusService(new[] { user }, new[] { company }, new[] { job }, new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) }, new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) }, new[] { match });

        var applicants = await service.GetApplicantsForCompanyAsync(company.CompanyId);

        applicants.Should().ContainSingle();
        applicants[0].Match.MatchId.Should().Be(match.MatchId);
        applicants[0].CompatibilityScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_WhenApplicantExists_ReturnsSingleApplicant()
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Rejected, "nope");

        var service = CreateCompanyStatusService(new[] { user }, new[] { company }, new[] { job }, new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) }, new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) }, new[] { match });

        var applicant = await service.GetApplicantByMatchIdAsync(company.CompanyId, match.MatchId);

        applicant.Should().NotBeNull();
        applicant!.Match.MatchId.Should().Be(match.MatchId);
    }

    private static UserStatusService CreateUserStatusService(
        IReadOnlyList<User> users,
        IReadOnlyList<Company> companies,
        IReadOnlyList<Job> jobs,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Match> matches)
    {
        return new UserStatusService(
            new FakeUserStatusMatchRepository(matches),
            new FakeJobService(new FakeJobRepository(jobs)),
            new FakeCompanyService(new FakeCompanyRepository(companies)),
            new FakeSkillService(new FakeSkillRepository(skills)),
            new FakeJobSkillService(new FakeJobSkillRepository(jobSkills)));
    }

    private static SkillGapService CreateSkillGapService(
        IReadOnlyList<User> users,
        IReadOnlyList<Match> matches,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills)
    {
        return new SkillGapService(
            new FakeUserStatusMatchRepository(matches),
            new FakeJobSkillService(new FakeJobSkillRepository(jobSkills)),
            new FakeSkillService(new FakeSkillRepository(skills)));
    }

    private static CompanyStatusService CreateCompanyStatusService(
        IReadOnlyList<User> users,
        IReadOnlyList<Company> companies,
        IReadOnlyList<Job> jobs,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Match> matches)
    {
        return new CompanyStatusService(
            new MatchService(new FakeMatchRepository(matches), new FakeJobService(new FakeJobRepository(jobs))),
            new FakeUserService(new FakeUserRepository(users)),
            new FakeJobService(new FakeJobRepository(jobs)),
            new FakeSkillService(new FakeSkillRepository(skills)));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly IReadOnlyList<User> users;
        public FakeUserRepository(IReadOnlyList<User> users) => this.users = users;
        public User? GetById(int userId) => users.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => users;
        public void Add(User user)
        {
        }

        public void Update(User user)
        {
        }

        public void Remove(int userId)
        {
        }
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        private readonly IReadOnlyList<Company> companies;
        public FakeCompanyRepository(IReadOnlyList<Company> companies) => this.companies = companies;
        public Company? GetById(int companyId) => companies.FirstOrDefault(company => company.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => companies;
        public void Add(Company company)
        {
        }

        public void Update(Company company)
        {
        }

        public void Remove(int companyId)
        {
        }
    }

    private sealed class FakeCompanyService : ICompanyService
    {
        private readonly ICompanyRepository companyRepository;
        public FakeCompanyService(ICompanyRepository companyRepository) => this.companyRepository = companyRepository;
        public Company? GetById(int companyId) => companyRepository.GetById(companyId);
        public IReadOnlyList<Company> GetAll() => companyRepository.GetAll();
        public void Add(Company company) => companyRepository.Add(company);
        public void Update(Company company) => companyRepository.Update(company);
        public void Remove(int companyId) => companyRepository.Remove(companyId);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        private readonly IReadOnlyList<Job> jobs;
        public FakeJobRepository(IReadOnlyList<Job> jobs) => this.jobs = jobs;
        public Job? GetById(int jobId) => jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job)
        {
        }

        public void Update(Job job)
        {
        }

        public void Remove(int jobId)
        {
        }
    }

    private sealed class FakeSkillRepository : ISkillRepository
    {
        private readonly IReadOnlyList<Skill> skills;
        public FakeSkillRepository(IReadOnlyList<Skill> skills) => this.skills = skills;
        public Skill? GetById(int userId, int skillId) => skills.FirstOrDefault(skill => skill.UserId == userId && skill.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => skills;
        public IReadOnlyList<Skill> GetByUserId(int userId) => skills.Where(skill => skill.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skills.GroupBy(skill => skill.SkillId).Select(group => (group.Key, group.First().SkillName)).ToList();
        public void Add(Skill skill)
        {
        }

        public void Update(Skill skill)
        {
        }

        public void Remove(int userId, int skillId)
        {
        }
    }

    private sealed class FakeJobSkillRepository : IJobSkillRepository
    {
        private readonly IReadOnlyList<JobSkill> jobSkills;
        public FakeJobSkillRepository(IReadOnlyList<JobSkill> jobSkills) => this.jobSkills = jobSkills;
        public JobSkill? GetById(int jobId, int skillId) => jobSkills.FirstOrDefault(jobSkill => jobSkill.JobId == jobId && jobSkill.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => jobSkills;
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => jobSkills.Where(jobSkill => jobSkill.JobId == jobId).ToList();
        public void Add(JobSkill jobSkill)
        {
        }

        public void Update(JobSkill jobSkill)
        {
        }

        public void Remove(int jobId, int skillId)
        {
        }
    }

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly List<Match> matches;
        public FakeMatchRepository(IReadOnlyList<Match> matches) => this.matches = matches.ToList();
        public Match? GetById(int matchId) => matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => matches;
        public void Add(Match match) => matches.Add(match);
        public void Update(Match match)
        {
        }

        public void Remove(int matchId)
        {
        }
        public int InsertReturningId(Match match) => 1;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
    }

    private sealed class FakeUserStatusMatchRepository : IUserStatusMatchRepository
    {
        private readonly IReadOnlyList<Match> matches;

        public FakeUserStatusMatchRepository(IReadOnlyList<Match> matches) => this.matches = matches;

        public IReadOnlyList<Match> GetByUserId(int userId) => matches.Where(match => match.UserId == userId).ToList();

        public IReadOnlyList<Match> GetRejectedByUserId(int userId) => matches.Where(match => match.UserId == userId && match.Status == MatchStatus.Rejected).ToList();
    }

    private sealed class FakeUserService : IUserService
    {
        private readonly IUserRepository userRepository;
        public FakeUserService(IUserRepository userRepository) => this.userRepository = userRepository;
        public User? GetById(int userId) => userRepository.GetById(userId);
        public IReadOnlyList<User> GetAll() => userRepository.GetAll();
        public void Add(User user) => userRepository.Add(user);
        public void Update(User user) => userRepository.Update(user);
        public void Remove(int userId) => userRepository.Remove(userId);
    }

    private sealed class FakeJobService : IJobService
    {
        private readonly IJobRepository jobRepository;
        public FakeJobService(IJobRepository jobRepository) => this.jobRepository = jobRepository;
        public Job? GetById(int jobId) => jobRepository.GetById(jobId);
        public IReadOnlyList<Job> GetAll() => jobRepository.GetAll();
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobRepository.GetByCompanyId(companyId);
        public void Add(Job job) => jobRepository.Add(job);
        public void Update(Job job) => jobRepository.Update(job);
        public void Remove(int jobId) => jobRepository.Remove(jobId);
    }

    private sealed class FakeSkillService : ISkillService
    {
        private readonly ISkillRepository skillRepository;
        public FakeSkillService(ISkillRepository skillRepository) => this.skillRepository = skillRepository;
        public Skill? GetById(int userId, int skillId) => skillRepository.GetById(userId, skillId);
        public IReadOnlyList<Skill> GetAll() => skillRepository.GetAll();
        public IReadOnlyList<Skill> GetByUserId(int userId) => skillRepository.GetByUserId(userId);
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skillRepository.GetDistinctSkillCatalog();
        public void Add(Skill skill) => skillRepository.Add(skill);
        public void Update(Skill skill) => skillRepository.Update(skill);
        public void Remove(int userId, int skillId) => skillRepository.Remove(userId, skillId);
    }

    private sealed class FakeJobSkillService : IJobSkillService
    {
        private readonly IJobSkillRepository jobSkillRepository;
        public FakeJobSkillService(IJobSkillRepository jobSkillRepository) => this.jobSkillRepository = jobSkillRepository;
        public JobSkill? GetById(int jobId, int skillId) => jobSkillRepository.GetById(jobId, skillId);
        public IReadOnlyList<JobSkill> GetAll() => jobSkillRepository.GetAll();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => jobSkillRepository.GetByJobId(jobId);
        public void Add(JobSkill jobSkill) => jobSkillRepository.Add(jobSkill);
        public void Update(JobSkill jobSkill) => jobSkillRepository.Update(jobSkill);
        public void Remove(int jobId, int skillId) => jobSkillRepository.Remove(jobId, skillId);
    }
}
