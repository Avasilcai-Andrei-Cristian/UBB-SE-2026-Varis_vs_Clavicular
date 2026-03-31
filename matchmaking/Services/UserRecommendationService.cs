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
        var jobs = _jobRepository.GetAll().Where(j => PassesFilters(j, filters, user)).ToList();

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

        var displayRec = new Recommendation
        {
            UserId = userId,
            JobId = best.Job.JobId,
            Timestamp = DateTime.UtcNow
        };
        var displayId = _recommendationRepository.InsertReturningId(displayRec);

        return new JobRecommendationResult
        {
            Job = best.Job,
            Company = company,
            CompatibilityScore = best.Score,
            TopSkillLabels = topSkills,
            AllSkillLabels = allSkillLabels,
            DisplayRecommendationId = displayId
        };
    }

    public JobRecommendationResult? RefreshDisplayedCard(
        int userId,
        JobRecommendationResult current,
        UserMatchmakingFilters filters)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new InvalidOperationException("User not found.");

        var job = _jobRepository.GetById(current.Job.JobId);
        if (job is null)
        {
            return null;
        }

        if (!PassesFilters(job, filters, user))
        {
            return null;
        }

        if (_matchService.GetByUserIdAndJobId(userId, job.JobId) is not null)
        {
            return null;
        }

        var userSkills = _skillRepository.GetByUserId(userId).ToList();
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
        var company = _companyRepository.GetById(job.CompanyId)
            ?? throw new InvalidOperationException($"Company {job.CompanyId} not found.");

        var jobSkillRows = skillsForRanking.ToList();
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
            DisplayRecommendationId = current.DisplayRecommendationId
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

    public void UndoLike(int matchId, int? displayRecommendationId)
    {
        _matchService.RemoveApplication(matchId);
        if (displayRecommendationId is { } rid)
        {
            _recommendationRepository.Remove(rid);
        }
    }

    public void UndoDismiss(int dismissRecommendationId, int? displayRecommendationId)
    {
        _recommendationRepository.Remove(dismissRecommendationId);
        if (displayRecommendationId is { } did && did != dismissRecommendationId)
        {
            _recommendationRepository.Remove(did);
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
            var jobSkillIds = _jobSkillRepository.GetByJobId(job.JobId).Select(js => js.SkillId).ToHashSet();
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
