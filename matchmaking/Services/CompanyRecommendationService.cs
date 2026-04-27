using System.Collections.Generic;
using System.Linq;
using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Services;

public class CompanyRecommendationService : ICompanyRecommendationService
{
    private readonly IMatchService matchService;
    private readonly IUserService userService;
    private readonly IJobService jobService;
    private readonly ISkillService skillService;
    private readonly IJobSkillService jobSkillService;
    private readonly IRecommendationAlgorithm algorithm;

    private List<UserApplicationResult> queue = new List<UserApplicationResult>();
    private int currentIndex;

    public CompanyRecommendationService(
        IMatchService matchService,
        IUserService userService,
        IJobService jobService,
        ISkillService skillService,
        IJobSkillService jobSkillService,
        IRecommendationAlgorithm algorithm)
    {
        this.matchService = matchService;
        this.userService = userService;
        this.jobService = jobService;
        this.skillService = skillService;
        this.jobSkillService = jobSkillService;
        this.algorithm = algorithm;
    }

    public void LoadApplicants(int companyId)
    {
        var companyJobs = jobService.GetByCompanyId(companyId);
        var companyJobIds = GetJobIds(companyJobs);

        if (companyJobIds.Count == 0)
        {
            queue = new List<UserApplicationResult>();
            currentIndex = 0;
            return;
        }

        var allMatches = matchService.GetAllMatches();
        var appliedMatches = new List<Match>();
        foreach (var match in allMatches)
        {
            if (match.Status == MatchStatus.Applied && companyJobIds.Contains(match.JobId))
            {
                appliedMatches.Add(match);
            }
        }

        var results = new List<UserApplicationResult>();
        foreach (var match in appliedMatches)
        {
            var user = userService.GetById(match.UserId);
            var job = jobService.GetById(match.JobId);
            if (user is null || job is null)
            {
                continue;
            }

            var userSkills = skillService.GetByUserId(user.UserId).ToList();
            var jobSkills = MapJobSkillsToSkills(job.JobId);

            var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

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

        results.Sort(CompareByCompatibilityScoreDescending);
        queue = results;
        currentIndex = 0;
    }

    public UserApplicationResult? GetNextApplicant()
    {
        if (currentIndex >= queue.Count)
        {
            return null;
        }

        return queue[currentIndex];
    }

    public void MoveToNext()
    {
        currentIndex++;
    }

    public void MoveToPrevious()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
        }
    }

    public bool HasMore => currentIndex < queue.Count;

    public CompatibilityBreakdown? GetBreakdown(UserApplicationResult applicant)
    {
        var jobSkills = MapJobSkillsToSkills(applicant.Job.JobId);

        return algorithm.CalculateScoreBreakdown(
            applicant.User,
            applicant.Job,
            applicant.UserSkills.ToList(),
            jobSkills);
    }

    private List<Skill> MapJobSkillsToSkills(int jobId)
    {
        var mapped = new List<Skill>();
        foreach (var jobSkill in jobSkillService.GetByJobId(jobId))
        {
            mapped.Add(new Skill
            {
                SkillId = jobSkill.SkillId,
                SkillName = jobSkill.SkillName,
                Score = jobSkill.Score
            });
        }

        return mapped;
    }

    private static HashSet<int> GetJobIds(IReadOnlyList<Job> companyJobs)
    {
        var jobIds = new HashSet<int>();
        foreach (var job in companyJobs)
        {
            jobIds.Add(job.JobId);
        }

        return jobIds;
    }

    private static int CompareByCompatibilityScoreDescending(UserApplicationResult left, UserApplicationResult right)
    {
        return right.CompatibilityScore.CompareTo(left.CompatibilityScore);
    }
}
