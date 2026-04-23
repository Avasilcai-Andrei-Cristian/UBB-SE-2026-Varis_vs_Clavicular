using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class JobSkillService : IJobSkillService
{
    private readonly IJobSkillRepository _jobSkillRepository;

    public JobSkillService(IJobSkillRepository jobSkillRepository)
    {
        _jobSkillRepository = jobSkillRepository;
    }

    public JobSkill? GetById(int jobId, int skillId) => _jobSkillRepository.GetById(jobId, skillId);
    public IReadOnlyList<JobSkill> GetAll() => _jobSkillRepository.GetAll();
    public IReadOnlyList<JobSkill> GetByJobId(int jobId) => _jobSkillRepository.GetByJobId(jobId);
    public void Add(JobSkill jobSkill) => _jobSkillRepository.Add(jobSkill);
    public void Update(JobSkill jobSkill) => _jobSkillRepository.Update(jobSkill);
    public void Remove(int jobId, int skillId) => _jobSkillRepository.Remove(jobId, skillId);
}
