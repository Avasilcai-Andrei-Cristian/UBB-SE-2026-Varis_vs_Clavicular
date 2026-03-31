using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.DTOs;

public class UserApplicationResult
{
    public required User User { get; set; }
    public required Match Match { get; set; }
    public required Job Job { get; set; }
    public double CompatibilityScore { get; set; }
    public IReadOnlyList<Skill> UserSkills { get; set; } = [];
    public string Feedback { get; set; } = string.Empty;
}
