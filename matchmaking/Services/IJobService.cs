using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Services;

public interface IJobService
{
    Job? GetById(int jobId);
    IReadOnlyList<Job> GetAll();
    IReadOnlyList<Job> GetByCompanyId(int companyId);
    void Add(Job job);
    void Update(Job job);
    void Remove(int jobId);
}
