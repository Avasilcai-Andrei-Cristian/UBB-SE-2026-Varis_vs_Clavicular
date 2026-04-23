using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class JobSkillService : IJobSkillService
{
    private readonly IJobSkillRepository jobSkillRepository;

    public JobSkillService(IJobSkillRepository jobSkillRepository)
    {
        this.jobSkillRepository = jobSkillRepository;
    }

    public JobSkill? GetById(int jobId, int skillId) => jobSkillRepository.GetById(jobId, skillId);
    public IReadOnlyList<JobSkill> GetAll() => jobSkillRepository.GetAll();
    public IReadOnlyList<JobSkill> GetByJobId(int jobId) => jobSkillRepository.GetByJobId(jobId);
    public void Add(JobSkill jobSkill) => jobSkillRepository.Add(jobSkill);
    public void Update(JobSkill jobSkill) => jobSkillRepository.Update(jobSkill);
    public void Remove(int jobId, int skillId) => jobSkillRepository.Remove(jobId, skillId);
}
