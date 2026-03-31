using System.Collections.Generic;
using System.Linq;
using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Services;

public class CompanyRecommendationService
{
    private readonly MatchService _matchService;
    private readonly UserService _userService;
    private readonly JobService _jobService;
    private readonly SkillService _skillService;
    private readonly JobSkillService _jobSkillService;
    private readonly RecommendationAlgorithm _algorithm;

    private List<UserApplicationResult> _queue = [];
    private int _currentIndex;

    public CompanyRecommendationService(
        MatchService matchService,
        UserService userService,
        JobService jobService,
        SkillService skillService,
        JobSkillService jobSkillService,
        RecommendationAlgorithm algorithm)
    {
        _matchService = matchService;
        _userService = userService;
        _jobService = jobService;
        _skillService = skillService;
        _jobSkillService = jobSkillService;
        _algorithm = algorithm;
    }

    public void LoadApplicants(int companyId)
    {
        var companyJobs = _jobService.GetByCompanyId(companyId);
        var companyJobIds = companyJobs.Select(j => j.JobId).ToHashSet();

        if (companyJobIds.Count == 0)
        {
            _queue = [];
            _currentIndex = 0;
            return;
        }

        var allMatches = _matchService.GetAllMatches();
        var appliedMatches = allMatches
            .Where(m => m.Status == MatchStatus.Applied && companyJobIds.Contains(m.JobId))
            .ToList();

        var results = new List<UserApplicationResult>();
        foreach (var match in appliedMatches)
        {
            var user = _userService.GetById(match.UserId);
            var job = _jobService.GetById(match.JobId);
            if (user is null || job is null)
            {
                continue;
            }

            var userSkills = _skillService.GetByUserId(user.UserId).ToList();
            var jobSkills = MapJobSkillsToSkills(job.JobId);

            var score = _algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

            results.Add(new UserApplicationResult
            {
                User = user,
                Match = match,
                Job = job,
                CompatibilityScore = score,
                UserSkills = userSkills,
                Feedback = match.FeedbackMessage
            });
        }

        _queue = results.OrderByDescending(r => r.CompatibilityScore).ToList();
        _currentIndex = 0;
    }

    public UserApplicationResult? GetNextApplicant()
    {
        if (_currentIndex >= _queue.Count)
        {
            return null;
        }

        return _queue[_currentIndex];
    }

    public void MoveToNext()
    {
        _currentIndex++;
    }

    public void MoveToPrevious()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
        }
    }

    public bool HasMore => _currentIndex < _queue.Count;

    public CompatibilityBreakdown? GetBreakdown(UserApplicationResult applicant)
    {
        var jobSkills = MapJobSkillsToSkills(applicant.Job.JobId);

        return _algorithm.CalculateScoreBreakdown(
            applicant.User,
            applicant.Job,
            applicant.UserSkills.ToList(),
            jobSkills);
    }

    private List<Skill> MapJobSkillsToSkills(int jobId)
    {
        return _jobSkillService.GetByJobId(jobId)
            .Select(js => new Skill { SkillId = js.SkillId, SkillName = js.SkillName, Score = js.Score })
            .ToList();
    }
}
