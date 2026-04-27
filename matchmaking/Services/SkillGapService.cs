using System.Collections.Generic;
using System.Linq;
using matchmaking.Models;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class SkillGapService : ISkillGapService
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

        var userSkillIds = new HashSet<int>();
        foreach (var userSkill in skillService.GetByUserId(userId))
        {
            userSkillIds.Add(userSkill.SkillId);
        }

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

        var missingSkills = new List<MissingSkillModel>();
        foreach (var missing in missingCount)
        {
            missingSkills.Add(new MissingSkillModel { SkillName = missing.Key, RejectedJobCount = missing.Value });
        }

        missingSkills.Sort(CompareMissingSkillCountDescending);
        return missingSkills;
    }

    public IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId)
    {
        var rejectedMatches = matchRepository.GetRejectedByUserId(userId);
        if (rejectedMatches.Count == 0)
        {
            return new List<UnderscoredSkillModel>();
        }

        var userSkillMap = new Dictionary<int, Domain.Entities.Skill>();
        foreach (var userSkill in skillService.GetByUserId(userId))
        {
            userSkillMap[userSkill.SkillId] = userSkill;
        }

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

        var underscoredSkills = new List<UnderscoredSkillModel>();
        foreach (var skill in requiredScoresPerSkill)
        {
            underscoredSkills.Add(new UnderscoredSkillModel
            {
                SkillName = skill.Value.Name,
                UserScore = skill.Value.UserScore,
                AverageRequiredScore = ComputeAverage(skill.Value.RequiredScores)
            });
        }

        underscoredSkills.Sort(CompareSkillGapDescending);
        return underscoredSkills;
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

    private static int CompareMissingSkillCountDescending(MissingSkillModel left, MissingSkillModel right)
    {
        return right.RejectedJobCount.CompareTo(left.RejectedJobCount);
    }

    private static int CompareSkillGapDescending(UnderscoredSkillModel left, UnderscoredSkillModel right)
    {
        var leftGap = left.AverageRequiredScore - left.UserScore;
        var rightGap = right.AverageRequiredScore - right.UserScore;
        return rightGap.CompareTo(leftGap);
    }

    private static int ComputeAverage(IReadOnlyList<int> values)
    {
        var sum = 0;
        foreach (var value in values)
        {
            sum += value;
        }

        return sum / values.Count;
    }
}
