using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Services;

public class CompanyStatusService : ICompanyStatusService
{
    private readonly IMatchService matchService;
    private readonly IUserService userService;
    private readonly IJobService jobService;
    private readonly ISkillService skillService;

    public CompanyStatusService(
        IMatchService matchService,
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
        var visibleMatches = new List<Match>();
        foreach (var match in matches)
        {
            if (IsVisibleMatch(match))
            {
                visibleMatches.Add(match);
            }
        }

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

        results.Sort(CompareByCompatibilityScoreDescending);
        var ordered = results;

        return ordered;
    }

    public async Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId)
    {
        var applicants = await GetApplicantsForCompanyAsync(companyId);
        foreach (var result in applicants)
        {
            if (result.Match.MatchId == matchId)
            {
                return result;
            }
        }

        return null;
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

        var averageSkillScore = ComputeAverageSkillScore(userSkills);
        var locationBonus = user.Location.Equals(job.Location, System.StringComparison.OrdinalIgnoreCase) ? 10 : 0;
        var employmentTypeBonus = user.PreferredEmploymentType.Equals(job.EmploymentType, System.StringComparison.OrdinalIgnoreCase)
            ? 10
            : 0;

        var computed = averageSkillScore + locationBonus + employmentTypeBonus;
        return computed > 100 ? 100 : computed;
    }

    private static bool IsVisibleMatch(Match match)
    {
        return match.Status is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
    }

    private static int CompareByCompatibilityScoreDescending(UserApplicationResult left, UserApplicationResult right)
    {
        return right.CompatibilityScore.CompareTo(left.CompatibilityScore);
    }

    private static double ComputeAverageSkillScore(IReadOnlyList<Skill> userSkills)
    {
        var sum = 0;
        foreach (var skill in userSkills)
        {
            sum += skill.Score;
        }

        return (double)sum / userSkills.Count;
    }
}
