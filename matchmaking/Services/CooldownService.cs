using System;
using matchmaking.Repositories;

namespace matchmaking.Services;

public sealed class CooldownService
{
    public static readonly TimeSpan UserJobDeckCooldown = TimeSpan.FromHours(24);

    private readonly SqlRecommendationRepository _recommendationRepository;

    public CooldownService(SqlRecommendationRepository recommendationRepository)
    {
        _recommendationRepository = recommendationRepository;
    }

    public bool IsOnCooldown(int userId, int jobId, DateTime utcNow)
    {
        var latest = _recommendationRepository.GetLatestByUserIdAndJobId(userId, jobId);
        if (latest is null)
        {
            return false;
        }

        var elapsed = utcNow - NormalizeToUtc(latest.Timestamp);
        return elapsed < UserJobDeckCooldown;
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
