using matchmaking.algorithm;
using matchmaking.Repositories;

namespace matchmaking.Services;

/// <summary>Wires repositories and services for the user matchmaking feature.</summary>
public static class MatchmakingComposition
{
    public static UserRecommendationService CreateUserRecommendationService(string connectionString)
    {
        var jobRepository = new JobRepository();
        var jobService = new JobService(jobRepository);
        var matchRepository = new SqlMatchRepository(connectionString);
        var matchService = new MatchService(matchRepository, jobService);
        var recommendationRepository = new SqlRecommendationRepository(connectionString);
        var cooldownService = new CooldownService(recommendationRepository);
        var algorithm = new RecommendationAlgorithm(
            new SqlPostRepository(connectionString),
            new SqlInteractionRepository(connectionString));

        return new UserRecommendationService(
            new UserRepository(),
            jobRepository,
            new SkillRepository(),
            new JobSkillRepository(),
            new CompanyRepository(),
            matchService,
            recommendationRepository,
            cooldownService,
            algorithm);
    }
}
