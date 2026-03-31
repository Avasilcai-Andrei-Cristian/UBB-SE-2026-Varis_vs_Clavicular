using System.Collections.Generic;

namespace matchmaking.DTOs;

/// <summary>
/// Filter options for the user matchmaking deck (requirements §2.1).
/// </summary>
public sealed class UserMatchmakingFilters
{
    /// <summary>Employment types to include (e.g. Full-time, Remote). Empty = no filter.</summary>
    public HashSet<string> EmploymentTypes { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Experience buckets: Internship, Entry, MidSenior, Director, Executive. Empty = no filter.</summary>
    public HashSet<string> ExperienceLevels { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string LocationSubstring { get; set; } = string.Empty;

    /// <summary>Required skill ids — job must require at least one. Empty = no filter.</summary>
    public HashSet<int> SkillIds { get; } = [];

    public static UserMatchmakingFilters Empty() => new();
}
