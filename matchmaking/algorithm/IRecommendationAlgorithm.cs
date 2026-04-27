using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.DTOs;

namespace matchmaking.algorithm;

public interface IRecommendationAlgorithm
{
    double CalculateCompatibilityScore(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills);

    CompatibilityBreakdown CalculateScoreBreakdown(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills);
}
