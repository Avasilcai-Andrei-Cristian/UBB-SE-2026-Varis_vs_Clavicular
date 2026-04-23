using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.DTOs;
using matchmaking.Repositories;

namespace matchmaking.Services;

public sealed class UserRecommendationService
{
    private readonly IUserRepository userRepository;
    private readonly IJobRepository jobRepository;
    private readonly ISkillRepository skillRepository;
    private readonly IJobSkillRepository jobSkillRepository;
    private readonly ICompanyRepository companyRepository;
    private readonly MatchService matchService;
    private readonly IRecommendationRepository recommendationRepository;
    private readonly CooldownService cooldownService;
    private readonly RecommendationAlgorithm algorithm;

    public UserRecommendationService(
        IUserRepository userRepository,
        IJobRepository jobRepository,
        ISkillRepository skillRepository,
        IJobSkillRepository jobSkillRepository,
        ICompanyRepository companyRepository,
        MatchService matchService,
        IRecommendationRepository recommendationRepository,
        CooldownService cooldownService,
        RecommendationAlgorithm algorithm)
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

        var (job, score) = ranked[0];
        return BuildCardWithShownRecord(userId, job, score);
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

        var jobs = jobRepository.GetAll().Where(j => PassesFilters(j, filters, user)).ToList();
        var userSkills = skillRepository.GetByUserId(userId).ToList();

        var ranked = new List<(Job Job, double Score)>();
        foreach (var job in jobs)
        {
            if (matchService.GetByUserIdAndJobId(userId, job.JobId) is not null)
            {
                continue;
            }

            var score = ComputeCompatibilityScore(user, job, userSkills, userId);
            ranked.Add((job, score));
        }

        return ranked.OrderByDescending(x => x.Score).ToList();
    }

    private double ComputeCompatibilityScore(User user, Job job, List<Skill> userSkills, int userId)
    {
        var skillsForRanking = jobSkillRepository.GetByJobId(job.JobId);
        var jobSkillsAsUserSkills = skillsForRanking
            .Select(js => new Skill
            {
                UserId = userId,
                SkillId = js.SkillId,
                SkillName = js.SkillName,
                Score = js.Score
            })
            .ToList();

        return algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkillsAsUserSkills);
    }

    private List<(Job Job, double Score)> BuildRankedList(int userId, UserMatchmakingFilters filters)
    {
        var user = userRepository.GetById(userId)
            ?? throw new InvalidOperationException("User not found.");

        var jobs = jobRepository.GetAll().Where(j => PassesFilters(j, filters, user)).ToList();
        var userSkills = skillRepository.GetByUserId(userId).ToList();

        var ranked = new List<(Job Job, double Score)>();
        foreach (var job in jobs)
        {
            if (matchService.GetByUserIdAndJobId(userId, job.JobId) is not null)
            {
                continue;
            }

            if (cooldownService.IsOnCooldown(userId, job.JobId, DateTime.UtcNow))
            {
                continue;
            }

            var score = ComputeCompatibilityScore(user, job, userSkills, userId);
            ranked.Add((job, score));
        }

        return ranked.OrderByDescending(x => x.Score).ToList();
    }

    private JobRecommendationResult BuildCardWithShownRecord(int userId, Job job, double score)
    {
        var displayRec = new Recommendation
        {
            UserId = userId,
            JobId = job.JobId,
            Timestamp = DateTime.UtcNow
        };

        var displayId = recommendationRepository.InsertReturningId(displayRec);
        return CreateCard(job, score, displayId);
    }

    private JobRecommendationResult CreateCard(Job job, double score, int? displayRecommendationId)
    {
        var company = companyRepository.GetById(job.CompanyId)
            ?? throw new InvalidOperationException($"Company {job.CompanyId} not found.");

        var jobSkillRows = jobSkillRepository.GetByJobId(job.JobId).ToList();
        var topSkills = JobRecommendationResult.TakeTopSkills(jobSkillRows);
        var allSkillLabels = jobSkillRows
            .Select(js => $"{js.SkillName} (min {js.Score})")
            .ToList();

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
        var job = card.Job;
        if (matchService.GetByUserIdAndJobId(userId, job.JobId) is not null)
        {
            throw new InvalidOperationException("Already applied to this job.");
        }

        return matchService.CreatePendingApplication(userId, job.JobId);
    }

    public int ApplyDismiss(int userId, JobRecommendationResult card)
    {
        var rec = new Recommendation
        {
            UserId = userId,
            JobId = card.Job.JobId,
            Timestamp = DateTime.UtcNow
        };

        return recommendationRepository.InsertReturningId(rec);
    }

    public void UndoLike(int matchId, int? displayRecommendationId)
    {
        matchService.RemoveApplication(matchId);
        if (displayRecommendationId is { } rid)
        {
            recommendationRepository.Remove(rid);
        }
    }

    public void UndoDismiss(int dismissRecommendationId, int? displayRecommendationId)
    {
        recommendationRepository.Remove(dismissRecommendationId);
        if (displayRecommendationId is { } did && did != dismissRecommendationId)
        {
            recommendationRepository.Remove(did);
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
            var jobSkillIds = jobSkillRepository.GetByJobId(job.JobId).Select(js => js.SkillId).ToHashSet();
            if (!filters.SkillIds.Any(id => jobSkillIds.Contains(id)))
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
}
