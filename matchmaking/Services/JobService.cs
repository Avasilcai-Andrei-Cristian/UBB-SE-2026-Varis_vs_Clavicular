using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public Job? GetById(int jobId) => _jobRepository.GetById(jobId);
    public IReadOnlyList<Job> GetAll() => _jobRepository.GetAll();
    public IReadOnlyList<Job> GetByCompanyId(int companyId) => _jobRepository.GetByCompanyId(companyId);
    public void Add(Job job) => _jobRepository.Add(job);
    public void Update(Job job) => _jobRepository.Update(job);
    public void Remove(int jobId) => _jobRepository.Remove(jobId);
}
