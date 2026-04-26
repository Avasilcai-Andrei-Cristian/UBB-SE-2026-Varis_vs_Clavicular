using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.DTOs;
using matchmaking.Repositories;

namespace matchmaking.Services;

public sealed class UserRecommendationService : IUserRecommendationService
{
    private readonly IUserRepository userRepository;
    private readonly IJobRepository jobRepository;
    private readonly ISkillRepository skillRepository;
    private readonly IJobSkillRepository jobSkillRepository;
    private readonly ICompanyRepository companyRepository;
    private readonly IMatchService matchService;
    private readonly IRecommendationRepository recommendationRepository;
    private readonly ICooldownService cooldownService;
    private readonly IRecommendationAlgorithm algorithm;

    public UserRecommendationService(
        IUserRepository userRepository,
        IJobRepository jobRepository,
        ISkillRepository skillRepository,
        IJobSkillRepository jobSkillRepository,
        ICompanyRepository companyRepository,
        IMatchService matchService,
        IRecommendationRepository recommendationRepository,
        ICooldownService cooldownService,
        IRecommendationAlgorithm algorithm)
    {
        this.userRepository = userRepository;
        this.jobRepository = jobRepository;
        this.skillRepository = skillRepository;
        this.jobSkillRepository = jobSkillRepository;
        this.companyRepository = companyRepository;
        this.matchService = matchService;
        this.recommendationRepository = recommendationRepository;
        this.cooldownService = cooldownService;
        this.algorithm = algorithm;
    }

    public JobRecommendationResult? GetNextCard(int userId, UserMatchmakingFilters filters)
    {
        var ranked = BuildRankedList(userId, filters);
        if (ranked.Count == 0)
        {
            return null;
        }

        var (topRankedJob, score) = ranked[0];
        return BuildCardWithShownRecord(userId, topRankedJob, score);
    }

    public JobRecommendationResult? RecalculateTopCardIgnoringCooldown(int userId, UserMatchmakingFilters filters)
    {
        var ranked = BuildRankedListIgnoringCooldown(userId, filters);
        if (ranked.Count == 0)
        {
            return null;
        }

        var best = ranked[0];
        return BuildCardWithShownRecord(userId, best.Job, best.Score);
    }

    private List<(Job Job, double Score)> BuildRankedListIgnoringCooldown(int userId, UserMatchmakingFilters filters)
    {
        var user = userRepository.GetById(userId)
            ?? throw new InvalidOperationException("User not found.");

        var jobs = GetFilteredJobs(filters, user);
        var userSkills = skillRepository.GetByUserId(userId).ToList();

        var ranked = new List<(Job Job, double Score)>();
        foreach (var currentJob in jobs)
        {
            if (matchService.GetByUserIdAndJobId(userId, currentJob.JobId) is not null)
            {
                continue;
            }

            var score = ComputeCompatibilityScore(user, currentJob, userSkills, userId);
            ranked.Add((currentJob, score));
        }

        ranked.Sort(CompareRankedJobsByScoreDescending);
        return ranked;
    }

    private double ComputeCompatibilityScore(User user, Job job, List<Skill> userSkills, int userId)
    {
        var skillsForRanking = jobSkillRepository.GetByJobId(job.JobId);
        var jobSkillsAsUserSkills = new List<Skill>();
        foreach (var jobSkill in skillsForRanking)
        {
            jobSkillsAsUserSkills.Add(new Skill
            {
                UserId = userId,
                SkillId = jobSkill.SkillId,
                SkillName = jobSkill.SkillName,
                Score = jobSkill.Score
            });
        }

        return algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkillsAsUserSkills);
    }

    private List<(Job Job, double Score)> BuildRankedList(int userId, UserMatchmakingFilters filters)
    {
        var user = userRepository.GetById(userId)
            ?? throw new InvalidOperationException("User not found.");

        var jobs = GetFilteredJobs(filters, user);
        var userSkills = skillRepository.GetByUserId(userId).ToList();

        var ranked = new List<(Job Job, double Score)>();
        foreach (var currentJob in jobs)
        {
            if (matchService.GetByUserIdAndJobId(userId, currentJob.JobId) is not null)
            {
                continue;
            }

            if (cooldownService.IsOnCooldown(userId, currentJob.JobId, DateTime.UtcNow))
            {
                continue;
            }

            var score = ComputeCompatibilityScore(user, currentJob, userSkills, userId);
            ranked.Add((currentJob, score));
        }

        ranked.Sort(CompareRankedJobsByScoreDescending);
        return ranked;
    }

    private JobRecommendationResult BuildCardWithShownRecord(int userId, Job job, double score)
    {
        var displayRecommendation = new Recommendation
        {
            UserId = userId,
            JobId = job.JobId,
            Timestamp = DateTime.UtcNow
        };

        var displayId = recommendationRepository.InsertReturningId(displayRecommendation);
        return CreateCard(job, score, displayId);
    }

    private JobRecommendationResult CreateCard(Job job, double score, int? displayRecommendationId)
    {
        var company = companyRepository.GetById(job.CompanyId)
            ?? throw new InvalidOperationException($"Company {job.CompanyId} not found.");

        var jobSkillRows = jobSkillRepository.GetByJobId(job.JobId).ToList();
        var topSkills = JobRecommendationResult.TakeTopSkills(jobSkillRows);
        var allSkillLabels = new List<string>();
        foreach (var jobSkill in jobSkillRows)
        {
            allSkillLabels.Add($"{jobSkill.SkillName} (min {jobSkill.Score})");
        }

        return new JobRecommendationResult
        {
            Job = job,
            Company = company,
            CompatibilityScore = score,
            TopSkillLabels = topSkills,
            AllSkillLabels = allSkillLabels,
            DisplayRecommendationId = displayRecommendationId
        };
    }

    public int ApplyLike(int userId, JobRecommendationResult card)
    {
        var targetJob = card.Job;
        if (matchService.GetByUserIdAndJobId(userId, targetJob.JobId) is not null)
        {
            throw new InvalidOperationException("Already applied to this job.");
        }

        return matchService.CreatePendingApplication(userId, targetJob.JobId);
    }

    public int ApplyDismiss(int userId, JobRecommendationResult card)
    {
        var dismissedRecommendation = new Recommendation
        {
            UserId = userId,
            JobId = card.Job.JobId,
            Timestamp = DateTime.UtcNow
        };

        return recommendationRepository.InsertReturningId(dismissedRecommendation);
    }

    public void UndoLike(int matchId, int? displayRecommendationId)
    {
        matchService.RemoveApplication(matchId);
        if (displayRecommendationId is { } resolvedDisplayRecommendationId)
        {
            recommendationRepository.Remove(resolvedDisplayRecommendationId);
        }
    }

    public void UndoDismiss(int dismissRecommendationId, int? displayRecommendationId)
    {
        recommendationRepository.Remove(dismissRecommendationId);
        if (displayRecommendationId is { } resolvedDisplayRecommendationId && resolvedDisplayRecommendationId != dismissRecommendationId)
        {
            recommendationRepository.Remove(resolvedDisplayRecommendationId);
        }
    }

    private bool PassesFilters(Job job, UserMatchmakingFilters filters, User user)
    {
        if (filters.EmploymentTypes.Count > 0)
        {
            if (!filters.EmploymentTypes.Contains(job.EmploymentType))
            {
                return false;
            }
        }

        if (filters.ExperienceLevels.Count > 0)
        {
            var bucket = MapUserYearsToExperienceBucket(user.YearsOfExperience);
            if (!filters.ExperienceLevels.Contains(bucket))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.LocationSubstring))
        {
            if (job.Location.IndexOf(filters.LocationSubstring.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }
        }

        if (filters.SkillIds.Count > 0)
        {
            var jobSkillIds = GetJobSkillIdSet(job.JobId);
            if (!HasAnySkillIntersection(filters.SkillIds, jobSkillIds))
            {
                return false;
            }
        }

        return true;
    }

    public static string MapUserYearsToExperienceBucket(int yearsOfExperience)
    {
        return yearsOfExperience switch
        {
            < 2 => "Internship",
            < 4 => "Entry",
            < 7 => "MidSenior",
            < 10 => "Director",
            _ => "Executive"
        };
    }

    private List<Job> GetFilteredJobs(UserMatchmakingFilters filters, User user)
    {
        var filteredJobs = new List<Job>();
        foreach (var job in jobRepository.GetAll())
        {
            if (PassesFilters(job, filters, user))
            {
                filteredJobs.Add(job);
            }
        }

        return filteredJobs;
    }

    private HashSet<int> GetJobSkillIdSet(int jobId)
    {
        var skillIds = new HashSet<int>();
        foreach (var jobSkill in jobSkillRepository.GetByJobId(jobId))
        {
            skillIds.Add(jobSkill.SkillId);
        }

        return skillIds;
    }

    private static bool HasAnySkillIntersection(IReadOnlyCollection<int> filterSkillIds, HashSet<int> jobSkillIds)
    {
        foreach (var filterSkillId in filterSkillIds)
        {
            if (jobSkillIds.Contains(filterSkillId))
            {
                return true;
            }
        }

        return false;
    }

    private static int CompareRankedJobsByScoreDescending((Job Job, double Score) left, (Job Job, double Score) right)
    {
        return right.Score.CompareTo(left.Score);
    }
}
