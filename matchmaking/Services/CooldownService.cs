using System;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

/// <summary>
/// Determines whether a user/job pair is still within the "seen" cooldown window
/// using <see cref="Recommendation"/> timestamps (requirements: not shown again until enough time passed).
/// Full configurability is deferred — period is hardcoded below.
/// </summary>
public sealed class CooldownService
{
    /// <summary>
    /// Hardcoded exclusion window after a dismiss (or any Recommendation row) for a user/job pair.
    /// Replace with configurable policy later.
    /// </summary>
    public static readonly TimeSpan UserJobDeckCooldown = TimeSpan.FromHours(24);

    private readonly SqlRecommendationRepository _recommendationRepository;

    public CooldownService(SqlRecommendationRepository recommendationRepository)
    {
        _recommendationRepository = recommendationRepository;
    }

    /// <returns>True if the job should be hidden from the deck due to a recent Recommendation row.</returns>
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
