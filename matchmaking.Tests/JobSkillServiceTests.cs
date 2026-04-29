using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class JobSkillServiceTests
{
    [Fact]
    public void GetByJobId_delegates_to_job_skill_repository()
    {
        const int targetJobId = 4;
        var jobSkillRepository = new FakeJobSkillRepository();
        jobSkillRepository.AllJobSkills.Add(new JobSkill { JobId = targetJobId, SkillId = 1, SkillName = "Docker" });
        jobSkillRepository.AllJobSkills.Add(new JobSkill { JobId = targetJobId, SkillId = 2, SkillName = "Kubernetes" });
        jobSkillRepository.AllJobSkills.Add(new JobSkill { JobId = 9, SkillId = 1, SkillName = "Docker" });
        var service = new JobSkillService(jobSkillRepository);
        var result = service.GetByJobId(targetJobId);
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(jobSkill => jobSkill.JobId.Should().Be(targetJobId));
    }

    private sealed class FakeJobSkillRepository : IJobSkillRepository
    {
        public List<JobSkill> AllJobSkills { get; } = [];
        public JobSkill? GetById(int jobId, int skillId) =>
            AllJobSkills.FirstOrDefault(jobSkill => jobSkill.JobId == jobId && jobSkill.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => AllJobSkills;
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) =>
            AllJobSkills.Where(jobSkill => jobSkill.JobId == jobId).ToList();
        public void Add(JobSkill jobSkill) => AllJobSkills.Add(jobSkill);
        public void Update(JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) =>
            AllJobSkills.RemoveAll(jobSkill => jobSkill.JobId == jobId && jobSkill.SkillId == skillId);
    }
}