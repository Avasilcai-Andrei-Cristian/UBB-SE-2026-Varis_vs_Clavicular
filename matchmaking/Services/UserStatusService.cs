using System;
using System.Collections.Generic;
using matchmaking.Models;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class UserStatusService : IUserStatusService
{
    private readonly IUserStatusMatchRepository matchRepository;
    private readonly IJobService jobService;
    private readonly ICompanyService companyService;
    private readonly ISkillService skillService;
    private readonly IJobSkillService jobSkillService;

    public UserStatusService(
        IUserStatusMatchRepository matchRepository,
        IJobService jobService,
        ICompanyService companyService,
        ISkillService skillService,
        IJobSkillService jobSkillService)
    {
        this.matchRepository = matchRepository;
        this.jobService = jobService;
        this.companyService = companyService;
        this.skillService = skillService;
        this.jobSkillService = jobSkillService;
    }

    public IReadOnlyList<ApplicationCardModel> GetApplicationsForUser(int userId)
    {
        var matches = matchRepository.GetByUserId(userId);
        var userSkills = skillService.GetByUserId(userId);
        var result = new List<ApplicationCardModel>();

        foreach (var match in matches)
        {
            var matchedJob = jobService.GetById(match.JobId);
            if (matchedJob is null)
            {
                continue;
            }

            var company = companyService.GetById(matchedJob.CompanyId);
            var jobSkills = jobSkillService.GetByJobId(match.JobId);
            var score = CalculateCompatibilityScore(userSkills, jobSkills);

            result.Add(new ApplicationCardModel
            {
                MatchId = match.MatchId,
                JobId = match.JobId,
                CompanyName = company?.CompanyName ?? "Unknown Company",
                JobDescription = matchedJob.JobDescription,
                AppliedDate = match.Timestamp,
                Status = match.Status,
                CompatibilityScore = score,
                FeedbackMessage = match.FeedbackMessage
            });
        }

        return result;
    }

    private static int CalculateCompatibilityScore(
        IReadOnlyList<Domain.Entities.Skill> userSkills,
        IReadOnlyList<Domain.Entities.JobSkill> jobSkills)
    {
        if (jobSkills.Count == 0)
        {
            return 100;
        }

        var userSkillMap = new Dictionary<int, int>();
        foreach (var skill in userSkills)
        {
            userSkillMap[skill.SkillId] = skill.Score;
        }

        double total = 0;
        foreach (var required in jobSkills)
        {
            if (userSkillMap.TryGetValue(required.SkillId, out var userScore))
            {
                total += Math.Min(userScore, required.Score) / (double)required.Score;
            }
        }

        return (int)(total / jobSkills.Count * 100);
    }
}
