namespace matchmaking.Tests;

public sealed class JobServiceTests
{
    [Fact]
    public void GetById_WhenJobExists_ReturnsJob()
    {
        var existingJob = TestDataFactory.CreateJob(21, 3);
        var repository = new FakeJobRepository(new[] { existingJob });
        var service = new JobService(repository);

        service.GetById(existingJob.JobId).Should().Be(existingJob);
    }

    [Fact]
    public void GetAll_WhenJobsExist_ReturnsJobs()
    {
        var existingJob = TestDataFactory.CreateJob(21, 3);
        var repository = new FakeJobRepository(new[] { existingJob });
        var service = new JobService(repository);

        service.GetAll().Should().ContainSingle().Which.Should().Be(existingJob);
    }

    [Fact]
    public void GetByCompanyId_WhenJobsExist_ReturnsJobs()
    {
        var existingJob = TestDataFactory.CreateJob(21, 3);
        var repository = new FakeJobRepository(new[] { existingJob });
        var service = new JobService(repository);

        service.GetByCompanyId(3).Should().ContainSingle().Which.Should().Be(existingJob);
    }

    [Fact]
    public void Add_WhenJobAdded_DelegatesToRepository()
    {
        var repository = new FakeJobRepository(Array.Empty<Job>());
        var service = new JobService(repository);
        var newJob = TestDataFactory.CreateJob(22, 3);

        service.Add(newJob);

        repository.AddedJobs.Should().ContainSingle().Which.Should().Be(newJob);
    }

    [Fact]
    public void Update_WhenJobUpdated_DelegatesToRepository()
    {
        var existingJob = TestDataFactory.CreateJob(21, 3);
        var repository = new FakeJobRepository(new[] { existingJob });
        var service = new JobService(repository);

        service.Update(existingJob);

        repository.UpdatedJobs.Should().ContainSingle().Which.Should().Be(existingJob);
    }

    [Fact]
    public void Remove_WhenJobRemoved_DelegatesToRepository()
    {
        var existingJob = TestDataFactory.CreateJob(21, 3);
        var repository = new FakeJobRepository(new[] { existingJob });
        var service = new JobService(repository);

        service.Remove(existingJob.JobId);

        repository.RemovedJobIds.Should().ContainSingle().Which.Should().Be(existingJob.JobId);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        private readonly List<Job> jobs;

        public FakeJobRepository(IReadOnlyList<Job> jobs)
        {
            this.jobs = jobs.ToList();
        }

        public List<Job> AddedJobs { get; } = new List<Job>();
        public List<Job> UpdatedJobs { get; } = new List<Job>();
        public List<int> RemovedJobIds { get; } = new List<int>();

        public Job? GetById(int jobId) => jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) => AddedJobs.Add(job);
        public void Update(Job job) => UpdatedJobs.Add(job);
        public void Remove(int jobId) => RemovedJobIds.Add(jobId);
    }
}
