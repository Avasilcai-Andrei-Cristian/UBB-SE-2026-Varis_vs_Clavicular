using System.Collections.Generic;
using System.Linq;
using matchmaking.Models;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class SkillGapService
{
    private readonly IUserStatusMatchRepository matchRepository;
    private readonly IJobSkillService jobSkillService;
    private readonly ISkillService skillService;

    public SkillGapService(
        IUserStatusMatchRepository matchRepository,
        IJobSkillService jobSkillService,
        ISkillService skillService)
    {
        this.matchRepository = matchRepository;
        this.jobSkillService = jobSkillService;
        this.skillService = skillService;
    }

    public IReadOnlyList<MissingSkillModel> GetMissingSkills(int userId)
    {
        var rejectedMatches = matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
        {
            return new List<MissingSkillModel>();
        }

        var userSkillIds = skillService.GetByUserId(userId)
            .Select(s => s.SkillId)
            .ToHashSet();

        var missingCount = new Dictionary<string, int>();
        foreach (var match in rejectedMatches)
        {
            foreach (var jobSkill in jobSkillService.GetByJobId(match.JobId))
            {
                if (!userSkillIds.Contains(jobSkill.SkillId))
                {
                    if (!missingCount.ContainsKey(jobSkill.SkillName))
                    {
                        missingCount[jobSkill.SkillName] = 0;
                    }

                    missingCount[jobSkill.SkillName]++;
                }
            }
        }

        return missingCount
            .Select(kv => new MissingSkillModel { SkillName = kv.Key, RejectedJobCount = kv.Value })
            .OrderByDescending(m => m.RejectedJobCount)
            .ToList();
    }

    public IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId)
    {
        var rejectedMatches = matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
        {
            return new List<UnderscoredSkillModel>();
        }

        var userSkillMap = skillService.GetByUserId(userId)
            .ToDictionary(s => s.SkillId, s => s);

        var requiredScoresPerSkill = new Dictionary<int, (string Name, int UserScore, List<int> RequiredScores)>();
        foreach (var match in rejectedMatches)
        {
            foreach (var jobSkill in jobSkillService.GetByJobId(match.JobId))
            {
                if (!userSkillMap.TryGetValue(jobSkill.SkillId, out var userSkill))
                {
                    continue;
                }

                if (userSkill.Score >= jobSkill.Score)
                {
                    continue;
                }

                if (!requiredScoresPerSkill.ContainsKey(jobSkill.SkillId))
                {
                    requiredScoresPerSkill[jobSkill.SkillId] = (jobSkill.SkillName, userSkill.Score, new List<int>());
                }

                requiredScoresPerSkill[jobSkill.SkillId].RequiredScores.Add(jobSkill.Score);
            }
        }

        return requiredScoresPerSkill
            .Select(kv => new UnderscoredSkillModel
            {
                SkillName = kv.Value.Name,
                UserScore = kv.Value.UserScore,
                AverageRequiredScore = (int)kv.Value.RequiredScores.Average()
            })
            .OrderByDescending(u => u.AverageRequiredScore - u.UserScore)
            .ToList();
    }

    public SkillGapSummaryModel GetSummary(int userId)
    {
        var rejectedMatches = matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
        {
            return new SkillGapSummaryModel { HasRejections = false, HasSkillGaps = false };
        }

        var missing = GetMissingSkills(userId);
        var underscored = GetUnderscoredSkills(userId);

        return new SkillGapSummaryModel
        {
            HasRejections = true,
            HasSkillGaps = missing.Count > 0 || underscored.Count > 0,
            MissingSkillsCount = missing.Count,
            SkillsToImproveCount = underscored.Count
        };
    }
}
