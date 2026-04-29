using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class JobSkillRepositoryTests
{
    [Fact]
    public void Add_attaches_skill_to_job_and_get_by_job_id_includes_it()
    {
        var repository = new JobSkillRepository(Enumerable.Empty<JobSkill>());
        const int jobId = 1;
        const int skillId = 5;
        var reactSkill = new JobSkill { JobId = jobId, SkillId = skillId, SkillName = "React", Score = 75 };
        repository.Add(reactSkill);
        var result = repository.GetByJobId(jobId);
        result.Should().ContainSingle();
        result[0].SkillId.Should().Be(skillId);
        result[0].SkillName.Should().Be("React");
        result[0].Score.Should().Be(75);
    }

    [Fact]
    public void GetByJobId_returns_all_skills_attached_to_specified_job()
    {
        const int targetJobId = 3;
        const int otherJobId = 7;
        var dockerSkill = new JobSkill { JobId = targetJobId, SkillId = 1, SkillName = "Docker", Score = 80 };
        var kubernetesSkill = new JobSkill { JobId = targetJobId, SkillId = 2, SkillName = "Kubernetes", Score = 70 };
        var unrelatedSkill = new JobSkill { JobId = otherJobId, SkillId = 3, SkillName = "Python", Score = 65 };
        var repository = new JobSkillRepository(new[] { dockerSkill, kubernetesSkill, unrelatedSkill });
        var result = repository.GetByJobId(targetJobId);
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(jobSkill => jobSkill.JobId.Should().Be(targetJobId));
    }

    [Fact]
    public void Remove_detaches_skill_from_job_so_it_no_longer_appears_in_get_by_job_id()
    {
        const int jobId = 2;
        const int skillId = 4;
        var sqlSkill = new JobSkill { JobId = jobId, SkillId = skillId, SkillName = "SQL", Score = 70 };
        var repository = new JobSkillRepository(new[] { sqlSkill });
        repository.Remove(jobId, skillId);
        var result = repository.GetByJobId(jobId);
        result.Should().BeEmpty();
    }
}