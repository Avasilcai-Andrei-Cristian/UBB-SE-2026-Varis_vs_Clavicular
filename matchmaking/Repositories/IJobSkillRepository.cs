using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IJobSkillRepository
{
    JobSkill? GetById(int jobId, int skillId);
    IReadOnlyList<JobSkill> GetAll();
    IReadOnlyList<JobSkill> GetByJobId(int jobId);
    void Add(JobSkill jobSkill);
    void Update(JobSkill jobSkill);
    void Remove(int jobId, int skillId);
}
