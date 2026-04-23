using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.algorithm;

public interface IRecommendationAlgorithm
{
    double CalculateCompatibilityScore(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills);
}
