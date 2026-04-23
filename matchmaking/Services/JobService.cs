using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class JobService : IJobService
{
    private readonly IJobRepository jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        this.jobRepository = jobRepository;
    }

    public Job? GetById(int jobId) => jobRepository.GetById(jobId);
    public IReadOnlyList<Job> GetAll() => jobRepository.GetAll();
    public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobRepository.GetByCompanyId(companyId);
    public void Add(Job job) => jobRepository.Add(job);
    public void Update(Job job) => jobRepository.Update(job);
    public void Remove(int jobId) => jobRepository.Remove(jobId);
}
