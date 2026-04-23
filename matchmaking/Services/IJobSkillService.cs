using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Services;

public interface IJobSkillService
{
    JobSkill? GetById(int jobId, int skillId);
    IReadOnlyList<JobSkill> GetAll();
    IReadOnlyList<JobSkill> GetByJobId(int jobId);
    void Add(JobSkill jobSkill);
    void Update(JobSkill jobSkill);
    void Remove(int jobId, int skillId);
}
