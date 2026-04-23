namespace matchmaking.Tests;

public sealed class JobSkillServiceTests
{
    [Fact]
    public void GetById_WhenJobSkillExists_ReturnsJobSkill()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository(new[] { existingJobSkill });
        var service = new JobSkillService(repository);

        service.GetById(existingJobSkill.JobId, existingJobSkill.SkillId).Should().Be(existingJobSkill);
    }

    [Fact]
    public void GetAll_WhenJobSkillsExist_ReturnsJobSkills()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository(new[] { existingJobSkill });
        var service = new JobSkillService(repository);

        service.GetAll().Should().ContainSingle().Which.Should().Be(existingJobSkill);
    }

    [Fact]
    public void GetByJobId_WhenJobSkillsExist_ReturnsJobSkills()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository(new[] { existingJobSkill });
        var service = new JobSkillService(repository);

        service.GetByJobId(existingJobSkill.JobId).Should().ContainSingle().Which.Should().Be(existingJobSkill);
    }

    [Fact]
    public void Add_WhenJobSkillAdded_DelegatesToRepository()
    {
        var repository = new FakeJobSkillRepository(Array.Empty<JobSkill>());
        var service = new JobSkillService(repository);
        var newJobSkill = TestDataFactory.CreateJobSkill(100, 11, "Docker", 70);

        service.Add(newJobSkill);

        repository.AddedItems.Should().ContainSingle().Which.Should().Be(newJobSkill);
    }

    [Fact]
    public void Update_WhenJobSkillUpdated_DelegatesToRepository()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository(new[] { existingJobSkill });
        var service = new JobSkillService(repository);

        service.Update(existingJobSkill);

        repository.UpdatedItems.Should().ContainSingle().Which.Should().Be(existingJobSkill);
    }

    [Fact]
    public void Remove_WhenJobSkillRemoved_DelegatesToRepository()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository(new[] { existingJobSkill });
        var service = new JobSkillService(repository);

        service.Remove(existingJobSkill.JobId, existingJobSkill.SkillId);

        repository.RemovedPairs.Should().ContainSingle().Which.Should().Be((existingJobSkill.JobId, existingJobSkill.SkillId));
    }

    private sealed class FakeJobSkillRepository : IJobSkillRepository
    {
        private readonly List<JobSkill> jobSkills;

        public FakeJobSkillRepository(IReadOnlyList<JobSkill> jobSkills)
        {
            this.jobSkills = jobSkills.ToList();
        }

        public List<JobSkill> AddedItems { get; } = new List<JobSkill>();
        public List<JobSkill> UpdatedItems { get; } = new List<JobSkill>();
        public List<(int JobId, int SkillId)> RemovedPairs { get; } = new List<(int JobId, int SkillId)>();

        public JobSkill? GetById(int jobId, int skillId) => jobSkills.FirstOrDefault(item => item.JobId == jobId && item.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => jobSkills;
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => jobSkills.Where(item => item.JobId == jobId).ToList();
        public void Add(JobSkill jobSkill) => AddedItems.Add(jobSkill);
        public void Update(JobSkill jobSkill) => UpdatedItems.Add(jobSkill);
        public void Remove(int jobId, int skillId) => RemovedPairs.Add((jobId, skillId));
    }
}
