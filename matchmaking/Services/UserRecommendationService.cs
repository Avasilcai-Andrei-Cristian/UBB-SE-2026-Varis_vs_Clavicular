using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;
using matchmaking.Repositories;

namespace matchmaking.Services;

/// <summary>
/// Ranks jobs for the user deck and records apply/skip actions (UML UserRecommendationService).
/// </summary>
public sealed class UserRecommendationService
{
    private readonly UserRepository _userRepository;
    private readonly JobRepository _jobRepository;
    private readonly SkillRepository _skillRepository;
    private readonly JobSkillRepository _jobSkillRepository;
    private readonly CompanyRepository _companyRepository;
    private readonly MatchService _matchService;
    private readonly SqlRecommendationRepository _recommendationRepository;
    private readonly CooldownService _cooldownService;
    private readonly RecommendationAlgorithm _algorithm;

    public UserRecommendationService(
        UserRepository userRepository,
        JobRepository jobRepository,
        SkillRepository skillRepository,
        JobSkillRepository jobSkillRepository,
        CompanyRepository companyRepository,
        MatchService matchService,
        SqlRecommendationRepository recommendationRepository,
        CooldownService cooldownService,
        RecommendationAlgorithm algorithm)
    {
        _userRepository = userRepository;
        _jobRepository = jobRepository;
        _skillRepository = skillRepository;
        _jobSkillRepository = jobSkillRepository;
        _companyRepository = companyRepository;
        _matchService = matchService;
        _recommendationRepository = recommendationRepository;
        _cooldownService = cooldownService;
        _algorithm = algorithm;
    }

    public JobRecommendationResult? GetNextCard(int userId, UserMatchmakingFilters filters)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new InvalidOperationException("User not found.");

        var userSkills = _skillRepository.GetByUserId(userId).ToList();
        var jobs = _jobRepository.GetAll().Where(j => PassesFilters(j, filters)).ToList();

        var ranked = new List<(Job Job, double Score)>();
        foreach (var job in jobs)
        {
            if (_matchService.GetByUserIdAndJobId(userId, job.JobId) is not null)
            {
                continue;
            }

            if (_cooldownService.IsOnCooldown(userId, job.JobId, DateTime.UtcNow))
            {
                continue;
            }

            var skillsForRanking = _jobSkillRepository.GetByJobId(job.JobId);
            var jobSkillsAsUserSkills = skillsForRanking
                .Select(js => new Skill
                {
                    UserId = userId,
                    SkillId = js.SkillId,
                    SkillName = js.SkillName,
                    Score = js.Score
                })
                .ToList();

            var score = _algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkillsAsUserSkills);
            ranked.Add((job, score));
        }

        if (ranked.Count == 0)
        {
            return null;
        }

        var best = ranked.OrderByDescending(x => x.Score).First();
        var company = _companyRepository.GetById(best.Job.CompanyId)
            ?? throw new InvalidOperationException($"Company {best.Job.CompanyId} not found.");

        var jobSkillRows = _jobSkillRepository.GetByJobId(best.Job.JobId).ToList();
        var topSkills = JobRecommendationResult.TakeTopSkills(jobSkillRows);
        var allSkillLabels = jobSkillRows
            .Select(js => $"{js.SkillName} (min {js.Score})")
            .ToList();

        return new JobRecommendationResult
        {
            Job = best.Job,
            Company = company,
            CompatibilityScore = best.Score,
            TopSkillLabels = topSkills,
            AllSkillLabels = allSkillLabels
        };
    }

    public int ApplyLike(int userId, JobRecommendationResult card)
    {
        var job = card.Job;
        if (_matchService.GetByUserIdAndJobId(userId, job.JobId) is not null)
        {
            throw new InvalidOperationException("Already applied to this job.");
        }

        return _matchService.CreatePendingApplication(userId, job.JobId);
    }

    public int ApplyDismiss(int userId, JobRecommendationResult card)
    {
        var rec = new Recommendation
        {
            UserId = userId,
            JobId = card.Job.JobId,
            Timestamp = DateTime.UtcNow
        };

        return _recommendationRepository.InsertReturningId(rec);
    }

    public void UndoDismiss(int recommendationId)
    {
        _recommendationRepository.Remove(recommendationId);
    }

    public void UndoLike(int matchId) => _matchService.RemoveApplication(matchId);

    private bool PassesFilters(Job job, UserMatchmakingFilters filters)
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
            var bucket = MapPromotionToExperienceBucket(job.PromotionLevel);
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
            var jobSkillIds = _jobSkillRepository.GetByJobId(job.JobId).Select(js => js.SkillId).ToHashSet();
            if (!filters.SkillIds.Any(id => jobSkillIds.Contains(id)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Maps promotion level to filter buckets (demo heuristic; requirements allow multi-select).</summary>
    public static string MapPromotionToExperienceBucket(int promotionLevel)
    {
        return promotionLevel switch
        {
            <= 20 => "Internship",
            <= 40 => "Entry",
            <= 60 => "MidSenior",
            <= 80 => "Director",
            _ => "Executive"
        };
    }
}
