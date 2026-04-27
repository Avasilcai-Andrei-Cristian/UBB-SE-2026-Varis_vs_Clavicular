using System;
using matchmaking.Repositories;

namespace matchmaking.Services;

public sealed class CooldownService : ICooldownService
{
    private readonly IRecommendationRepository recommendationRepository;
    private readonly TimeSpan cooldownPeriod;

    public CooldownService(IRecommendationRepository recommendationRepository, TimeSpan cooldownPeriod)
    {
        this.recommendationRepository = recommendationRepository;
        this.cooldownPeriod = cooldownPeriod <= TimeSpan.Zero ? TimeSpan.FromHours(24) : cooldownPeriod;
    }

    public bool IsOnCooldown(int userId, int jobId, DateTime utcNow)
    {
        var latest = recommendationRepository.GetLatestByUserIdAndJobId(userId, jobId);
        if (latest is null)
        {
            return false;
        }

        var elapsed = utcNow - NormalizeToUtc(latest.Timestamp);
        return elapsed < cooldownPeriod;
    }

    private static DateTime NormalizeToUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Local).ToUniversalTime()
        };
    }
}
