using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class JobRepository : IJobRepository
{
    private readonly List<Job> jobs;

    public JobRepository()
        : this(CreateDefaultJobs())
    {
    }

    public JobRepository(IEnumerable<Job> initialJobs)
    {
        jobs = initialJobs.ToList();
    }

    private static IEnumerable<Job> CreateDefaultJobs()
    {
        return [
        new ()
        {
            JobId = 100,
            JobTitle = "Backend Engineer",
            JobDescription =
                "Design and maintain REST APIs and internal microservices. Collaborate with product on requirements, code reviews, and production incidents.",
            Location = "Bucharest",
            EmploymentType = "Full-time",
            CompanyId = 1,
            PromotionLevel = 2
        },
        new ()
        {
            JobId = 101,
            JobTitle = "Python Developer",
            JobDescription =
                "Build data pipelines and backend services in Python. Work with analysts to ship reliable batch and streaming jobs.",
            Location = "Bucharest",
            EmploymentType = "Full-time",
            CompanyId = 1,
            PromotionLevel = 2
        },
        new ()
        {
            JobId = 102,
            JobTitle = "Frontend Engineer",
            JobDescription =
                "Implement responsive UIs with modern frameworks. Partner with designers and API teams to deliver accessible, performant features.",
            Location = "Bucharest",
            EmploymentType = "Hybrid",
            CompanyId = 1,
            PromotionLevel = 1
        },
        new ()
        {
            JobId = 103,
            JobTitle = "DevOps Engineer",
            JobDescription =
                "Own CI/CD, infrastructure as code, and observability. Improve deployment safety and developer experience across squads.",
            Location = "Bucharest",
            EmploymentType = "Remote",
            CompanyId = 1,
            PromotionLevel = 3
        },
        new ()
        {
            JobId = 104,
            JobTitle = "QA Engineer",
            JobDescription =
                "Plan and execute manual and automated tests. File clear defects and work with developers to improve quality gates.",
            Location = "Bucharest",
            EmploymentType = "Full-time",
            CompanyId = 1,
            PromotionLevel = 1
        },
        new ()
        {
            JobId = 1,
            JobTitle = "Junior Frontend Developer",
            JobDescription =
                "Ship UI features for our web app under mentorship. Learn React, testing, and our design system while pairing with senior engineers.",
            Location = "Cluj-Napoca",
            EmploymentType = "Full-time",
            CompanyId = 4,
            PromotionLevel = 1
        },
        new ()
        {
            JobId = 2,
            JobTitle = "Backend .NET Developer",
            JobDescription =
                "Join our Bucharest team building enterprise APIs and integrations. Strong C# and SQL; experience with Azure or containers is a plus.",
            Location = "Bucharest",
            EmploymentType = "Hybrid",
            CompanyId = 1,
            PromotionLevel = 2
        },
        new ()
        {
            JobId = 3,
            JobTitle = "QA Automation Engineer",
            JobDescription =
                "Expand our automated regression suite and integrate tests into CI. Comfortable with web drivers and API testing.",
            Location = "Iasi",
            EmploymentType = "Full-time",
            CompanyId = 8,
            PromotionLevel = 1
        },
        new ()
        {
            JobId = 4,
            JobTitle = "DevOps Engineer",
            JobDescription =
                "Manage Kubernetes clusters, pipelines, and secrets. On-call rotation with clear runbooks and blameless postmortems.",
            Location = "Timisoara",
            EmploymentType = "Remote",
            CompanyId = 2,
            PromotionLevel = 3
        },
        new ()
        {
            JobId = 5,
            JobTitle = "Data Analyst",
            JobDescription =
                "Turn business questions into dashboards and ad hoc analyses. SQL and visualization tools; curiosity about the domain.",
            Location = "Brasov",
            EmploymentType = "Hybrid",
            CompanyId = 3,
            PromotionLevel = 1
        },
        new ()
        {
            JobId = 6,
            JobTitle = "ML Engineer",
            JobDescription =
                "Train and evaluate models for production use. Collaborate on feature stores, monitoring drift, and responsible deployment.",
            Location = "Oradea",
            EmploymentType = "Full-time",
            CompanyId = 9,
            PromotionLevel = 2
        },
        new ()
        {
            JobId = 7,
            JobTitle = "UI/UX Designer",
            JobDescription =
                "Own flows from research to high-fidelity specs. Work closely with engineering to ship polished, usable interfaces.",
            Location = "Sibiu",
            EmploymentType = "Part-time",
            CompanyId = 7,
            PromotionLevel = 1
        },
        new ()
        {
            JobId = 8,
            JobTitle = "Technical Lead",
            JobDescription =
                "Lead a cross-functional team: architecture decisions, mentoring, and delivery planning. Hands-on when critical paths need it.",
            Location = "Constanta",
            EmploymentType = "Hybrid",
            CompanyId = 10,
            PromotionLevel = 4
        },
        new ()
        {
            JobId = 9,
            JobTitle = "Full-Stack Developer",
            JobDescription =
                "End-to-end features across API and SPA. Balance speed with tests, documentation, and operational readiness.",
            Location = "Craiova",
            EmploymentType = "Remote",
            CompanyId = 6,
            PromotionLevel = 2
        },
        new ()
        {
            JobId = 10,
            JobTitle = "Cloud Architect",
            JobDescription =
                "Define cloud landing zones, security patterns, and cost guardrails. Align stakeholders on multi-year platform roadmap.",
            Location = "Targu Mures",
            EmploymentType = "Full-time",
            CompanyId = 5,
            PromotionLevel = 5
        },
        new ()
        {
            JobId = 11,
            JobTitle = "C++ Architect",
            JobDescription =
                "Define cloud landing zones, security patterns, and cost guardrails. Align stakeholders on multi-year platform roadmap.",
            Location = "Targu Mures",
            EmploymentType = "Full-time",
            CompanyId = 5,
            PromotionLevel = 5
        },
        new ()
        {
            JobId = 12,
            JobTitle = "C-- Architect",
            JobDescription =
                "Define cloud landing zones, security patterns, and cost guardrails. Align stakeholders on multi-year platform roadmap.",
            Location = "Targu Mures",
            EmploymentType = "Full-time",
            CompanyId = 5,
            PromotionLevel = 5
        }
        ];
    }

    public Job? GetById(int jobId)
    {
        foreach (var job in jobs)
        {
            if (job.JobId == jobId)
            {
                return job;
            }
        }

        return null;
    }

    public IReadOnlyList<Job> GetAll() => jobs.ToList();

    public IReadOnlyList<Job> GetByCompanyId(int companyId)
    {
        var result = new List<Job>();
        foreach (var job in jobs)
        {
            if (job.CompanyId == companyId)
            {
                result.Add(job);
            }
        }

        return result;
    }

    public void Add(Job job)
    {
        if (HasJobId(job.JobId))
        {
            throw new InvalidOperationException($"Job with id {job.JobId} already exists.");
        }

        jobs.Add(job);
    }

    public void Update(Job job)
    {
        var existing = GetById(job.JobId) ?? throw new KeyNotFoundException($"Job with id {job.JobId} was not found.");
        existing.JobTitle = job.JobTitle;
        existing.JobDescription = job.JobDescription;
        existing.Location = job.Location;
        existing.EmploymentType = job.EmploymentType;
        existing.CompanyId = job.CompanyId;
        existing.PromotionLevel = job.PromotionLevel;
    }

    public void Remove(int jobId)
    {
        var existing = GetById(jobId) ?? throw new KeyNotFoundException($"Job with id {jobId} was not found.");
        jobs.Remove(existing);
    }

    private bool HasJobId(int jobId)
    {
        foreach (var job in jobs)
        {
            if (job.JobId == jobId)
            {
                return true;
            }
        }

        return false;
    }
}
