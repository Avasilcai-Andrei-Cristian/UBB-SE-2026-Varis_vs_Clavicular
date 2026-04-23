using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Services;

public class CompanyStatusService
{
    private readonly MatchService matchService;
    private readonly IUserService userService;
    private readonly IJobService jobService;
    private readonly ISkillService skillService;

    public CompanyStatusService(
        MatchService matchService,
        IUserService userService,
        IJobService jobService,
        ISkillService skillService)
    {
        this.matchService = matchService;
        this.userService = userService;
        this.jobService = jobService;
        this.skillService = skillService;
    }

    public async Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId)
    {
        var matches = await matchService.GetByCompanyIdAsync(companyId);
        var visibleMatches = matches
            .Where(match => match.Status is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced)
            .ToList();

        var results = new List<UserApplicationResult>(visibleMatches.Count);

        foreach (var match in visibleMatches)
        {
            var user = userService.GetById(match.UserId);
            var job = jobService.GetById(match.JobId);
            if (user is null || job is null)
            {
                continue;
            }

            var userSkills = skillService.GetByUserId(user.UserId);
            var result = BuildResult(match, user, job, userSkills);
            results.Add(result);
        }

        var ordered = results
            .OrderByDescending(result => result.CompatibilityScore)
            .ToList();

        return ordered;
    }

    public async Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId)
    {
        var applicants = await GetApplicantsForCompanyAsync(companyId);
        return applicants.FirstOrDefault(result => result.Match.MatchId == matchId);
    }

    private UserApplicationResult BuildResult(
        Match match,
        User user,
        Job job,
        IReadOnlyList<Skill> userSkills)
    {
        return new UserApplicationResult
        {
            User = user,
            Match = match,
            Job = job,
            CompatibilityScore = ComputeCompatibilityFallback(user, job, userSkills),
            UserSkills = userSkills,
            Feedback = match.FeedbackMessage
        };
    }

    private static double ComputeCompatibilityFallback(User user, Job job, IReadOnlyList<Skill> userSkills)
    {
        if (userSkills.Count == 0)
        {
            return 0;
        }

        var averageSkillScore = userSkills.Average(skill => skill.Score);
        var locationBonus = user.Location.Equals(job.Location, System.StringComparison.OrdinalIgnoreCase) ? 10 : 0;
        var employmentTypeBonus = user.PreferredEmploymentType.Equals(job.EmploymentType, System.StringComparison.OrdinalIgnoreCase)
            ? 10
            : 0;

        var computed = averageSkillScore + locationBonus + employmentTypeBonus;
        return computed > 100 ? 100 : computed;
    }
}
