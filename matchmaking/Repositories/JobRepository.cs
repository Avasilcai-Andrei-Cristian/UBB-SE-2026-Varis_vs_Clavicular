using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class JobRepository
{
    private readonly List<Job> _jobs =
    [
        new() { JobId = 100, JobDescription = "Backend Engineer", Location = "Bucharest", EmploymentType = "Full-time", CompanyId = 1, PromotionLevel = 2 },
        new() { JobId = 101, JobDescription = "Python Developer", Location = "Bucharest", EmploymentType = "Full-time", CompanyId = 1, PromotionLevel = 2 },
        new() { JobId = 102, JobDescription = "Frontend Engineer", Location = "Bucharest", EmploymentType = "Hybrid", CompanyId = 1, PromotionLevel = 1 },
        new() { JobId = 103, JobDescription = "DevOps Engineer", Location = "Bucharest", EmploymentType = "Remote", CompanyId = 1, PromotionLevel = 3 },
        new() { JobId = 104, JobDescription = "QA Engineer", Location = "Bucharest", EmploymentType = "Full-time", CompanyId = 1, PromotionLevel = 1 },
        new() { JobId = 1, JobDescription = "Junior Frontend Developer", Location = "Cluj-Napoca", EmploymentType = "Full-time", CompanyId = 4, PromotionLevel = 1 },
        new() { JobId = 2, JobDescription = "Backend .NET Developer", Location = "Bucharest", EmploymentType = "Hybrid", CompanyId = 1, PromotionLevel = 2 },
        new() { JobId = 3, JobDescription = "QA Automation Engineer", Location = "Iasi", EmploymentType = "Full-time", CompanyId = 8, PromotionLevel = 1 },
        new() { JobId = 4, JobDescription = "DevOps Engineer", Location = "Timisoara", EmploymentType = "Remote", CompanyId = 2, PromotionLevel = 3 },
        new() { JobId = 5, JobDescription = "Data Analyst", Location = "Brasov", EmploymentType = "Hybrid", CompanyId = 3, PromotionLevel = 1 },
        new() { JobId = 6, JobDescription = "ML Engineer", Location = "Oradea", EmploymentType = "Full-time", CompanyId = 9, PromotionLevel = 2 },
        new() { JobId = 7, JobDescription = "UI/UX Designer", Location = "Sibiu", EmploymentType = "Part-time", CompanyId = 7, PromotionLevel = 1 },
        new() { JobId = 8, JobDescription = "Technical Lead", Location = "Constanta", EmploymentType = "Hybrid", CompanyId = 10, PromotionLevel = 4 },
        new() { JobId = 9, JobDescription = "Full-Stack Developer", Location = "Craiova", EmploymentType = "Remote", CompanyId = 6, PromotionLevel = 2 },
        new() { JobId = 10, JobDescription = "Cloud Architect", Location = "Targu Mures", EmploymentType = "Full-time", CompanyId = 5, PromotionLevel = 5 }
    ];

    public Job? GetById(int jobId) => _jobs.FirstOrDefault(j => j.JobId == jobId);

    public IReadOnlyList<Job> GetAll() => _jobs.ToList();

    public IReadOnlyList<Job> GetByCompanyId(int companyId) =>
        _jobs.Where(j => j.CompanyId == companyId).ToList();

    public void Add(Job job)
    {
        if (_jobs.Any(j => j.JobId == job.JobId))
        {
            throw new InvalidOperationException($"Job with id {job.JobId} already exists.");
        }

        _jobs.Add(job);
    }

    public void Update(Job job)
    {
        var existing = GetById(job.JobId) ?? throw new KeyNotFoundException($"Job with id {job.JobId} was not found.");
        existing.JobDescription = job.JobDescription;
        existing.Location = job.Location;
        existing.EmploymentType = job.EmploymentType;
        existing.CompanyId = job.CompanyId;
        existing.PromotionLevel = job.PromotionLevel;
    }

    public void Remove(int jobId)
    {
        var existing = GetById(jobId) ?? throw new KeyNotFoundException($"Job with id {jobId} was not found.");
        _jobs.Remove(existing);
    }
}
