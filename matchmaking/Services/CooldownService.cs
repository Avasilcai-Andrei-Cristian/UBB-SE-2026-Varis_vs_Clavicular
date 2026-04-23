using System;
using matchmaking.Repositories;

namespace matchmaking.Services;

public sealed class CooldownService
{
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly TimeSpan _cooldownPeriod;

    public CooldownService(IRecommendationRepository recommendationRepository, TimeSpan cooldownPeriod)
    {
        _recommendationRepository = recommendationRepository;
        _cooldownPeriod = cooldownPeriod <= TimeSpan.Zero ? TimeSpan.FromHours(24) : cooldownPeriod;
    }

    public bool IsOnCooldown(int userId, int jobId, DateTime utcNow)
    {
        var latest = _recommendationRepository.GetLatestByUserIdAndJobId(userId, jobId);
        if (latest is null)
        {
            return false;
        }

        var elapsed = utcNow - NormalizeToUtc(latest.Timestamp);
        return elapsed < _cooldownPeriod;
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
