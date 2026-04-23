using System;
using System.Collections.Generic;
using matchmaking.Models;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class UserStatusService
{
    private readonly IUserStatusMatchRepository _matchRepository;
    private readonly IJobService _jobService;
    private readonly ICompanyService _companyService;
    private readonly ISkillService _skillService;
    private readonly IJobSkillService _jobSkillService;

    public UserStatusService(
        IUserStatusMatchRepository matchRepository,
        IJobService jobService,
        ICompanyService companyService,
        ISkillService skillService,
        IJobSkillService jobSkillService)
    {
        _matchRepository = matchRepository;
        _jobService      = jobService;
        _companyService  = companyService;
        _skillService    = skillService;
        _jobSkillService = jobSkillService;
    }

    public IReadOnlyList<ApplicationCardModel> GetApplicationsForUser(int userId)
    {
        var matches    = _matchRepository.GetByUserId(userId);
        var userSkills = _skillService.GetByUserId(userId);
        var result     = new List<ApplicationCardModel>();

        foreach (var match in matches)
        {
            var job = _jobService.GetById(match.JobId);
            if (job == null) continue;

            var company    = _companyService.GetById(job.CompanyId);
            var jobSkills  = _jobSkillService.GetByJobId(match.JobId);
            var score      = CalculateCompatibilityScore(userSkills, jobSkills);

            result.Add(new ApplicationCardModel
            {
                MatchId            = match.MatchId,
                JobId              = match.JobId,
                CompanyName        = company?.CompanyName ?? "Unknown Company",
                JobDescription     = job.JobDescription,
                AppliedDate        = match.Timestamp,
                Status             = match.Status,
                CompatibilityScore = score,
                FeedbackMessage    = match.FeedbackMessage
            });
        }

        return result;
    }

    private static int CalculateCompatibilityScore(
        IReadOnlyList<Domain.Entities.Skill>    userSkills,
        IReadOnlyList<Domain.Entities.JobSkill> jobSkills)
    {
        if (jobSkills.Count == 0) return 100;

        var userSkillMap = new Dictionary<int, int>();
        foreach (var skill in userSkills)
            userSkillMap[skill.SkillId] = skill.Score;

        double total = 0;
        foreach (var required in jobSkills)
        {
            if (userSkillMap.TryGetValue(required.SkillId, out var userScore))
                total += Math.Min(userScore, required.Score) / (double)required.Score;
        }

        return (int)(total / jobSkills.Count * 100);
    }
}
