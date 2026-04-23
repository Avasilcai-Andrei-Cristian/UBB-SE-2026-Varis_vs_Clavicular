using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class JobSkillRepository : IJobSkillRepository
{
    private readonly List<JobSkill> jobSkills =
    [
        new () { JobId = 1, SkillId = 2, SkillName = "React", Score = 70 },
        new () { JobId = 1, SkillId = 12, SkillName = "Figma", Score = 45 },
        new () { JobId = 2, SkillId = 1, SkillName = "C#", Score = 80 },
        new () { JobId = 2, SkillId = 3, SkillName = "SQL", Score = 75 },
        new () { JobId = 3, SkillId = 4, SkillName = "Testing", Score = 75 },
        new () { JobId = 3, SkillId = 5, SkillName = "Selenium", Score = 65 },
        new () { JobId = 4, SkillId = 6, SkillName = "Docker", Score = 78 },
        new () { JobId = 4, SkillId = 7, SkillName = "Kubernetes", Score = 72 },
        new () { JobId = 5, SkillId = 8, SkillName = "Python", Score = 68 },
        new () { JobId = 5, SkillId = 9, SkillName = "Pandas", Score = 62 },
        new () { JobId = 6, SkillId = 10, SkillName = "Machine Learning", Score = 80 },
        new () { JobId = 6, SkillId = 11, SkillName = "NLP", Score = 72 },
        new () { JobId = 7, SkillId = 12, SkillName = "Figma", Score = 70 },
        new () { JobId = 7, SkillId = 13, SkillName = "UI Design", Score = 75 },
        new () { JobId = 8, SkillId = 14, SkillName = "Architecture", Score = 85 },
        new () { JobId = 8, SkillId = 15, SkillName = "Leadership", Score = 80 },
        new () { JobId = 9, SkillId = 1, SkillName = "C#", Score = 72 },
        new () { JobId = 9, SkillId = 2, SkillName = "React", Score = 70 },
        new () { JobId = 10, SkillId = 16, SkillName = "Cloud", Score = 86 },
        new () { JobId = 10, SkillId = 6, SkillName = "Docker", Score = 73 }
    ];

    public JobSkill? GetById(int jobId, int skillId) =>
        jobSkills.FirstOrDefault(js => js.JobId == jobId && js.SkillId == skillId);

    public IReadOnlyList<JobSkill> GetAll() => jobSkills.ToList();

    public IReadOnlyList<JobSkill> GetByJobId(int jobId) =>
        jobSkills.Where(js => js.JobId == jobId).ToList();

    public void Add(JobSkill jobSkill)
    {
        if (jobSkills.Any(js => js.JobId == jobSkill.JobId && js.SkillId == jobSkill.SkillId))
        {
            throw new InvalidOperationException($"JobSkill ({jobSkill.JobId}, {jobSkill.SkillId}) already exists.");
        }

        jobSkills.Add(jobSkill);
    }

    public void Update(JobSkill jobSkill)
    {
        var existing = GetById(jobSkill.JobId, jobSkill.SkillId)
            ?? throw new KeyNotFoundException($"JobSkill ({jobSkill.JobId}, {jobSkill.SkillId}) was not found.");
        existing.SkillName = jobSkill.SkillName;
        existing.Score = jobSkill.Score;
    }

    public void Remove(int jobId, int skillId)
    {
        var existing = GetById(jobId, skillId)
            ?? throw new KeyNotFoundException($"JobSkill ({jobId}, {skillId}) was not found.");
        jobSkills.Remove(existing);
    }
}
