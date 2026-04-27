using System;
using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.DTOs;

public sealed class JobRecommendationResult
{
    public required Job Job { get; init; }
    public required Company Company { get; init; }
    public double CompatibilityScore { get; init; }

    public int? DisplayRecommendationId { get; init; }

    public string JobTitleLine
    {
        get
        {
            var title = Job.JobTitle.Trim();
            if (!string.IsNullOrEmpty(title))
            {
                return title.Length > 80 ? title[..80] + "..." : title;
            }

            var trimmedDescription = Job.JobDescription.Trim();
            if (string.IsNullOrEmpty(trimmedDescription))
            {
                return string.Empty;
            }

            var firstLine = GetFirstLine(trimmedDescription);
            return firstLine.Length > 80 ? firstLine[..80] + "..." : firstLine;
        }
    }

    public string DescriptionExcerpt => BuildExcerpt(Job.JobDescription, 150);

    public string LocationEmploymentLine => $"{Job.Location} - {Job.EmploymentType}";

    public string MatchScoreDisplay => $"{CompatibilityScore:0.#}%";

    public string MatchLineLabel => $"Match: {MatchScoreDisplay}";

    public IReadOnlyList<string> TopSkillLabels { get; init; } = new List<string>();

    public IReadOnlyList<string> AllSkillLabels { get; init; } = new List<string>();

    public string ContactLine => $"{Company.Email} - {Company.Phone}";

    public static string BuildExcerpt(string description, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var trimmed = description.Trim();
        if (trimmed.Length <= maxChars)
        {
            return trimmed;
        }

        return trimmed[..maxChars].TrimEnd() + "...";
    }

    public static IReadOnlyList<string> TakeTopSkills(IEnumerable<JobSkill> jobSkills, int count = 3)
    {
        var skillLabels = new List<string>();
        var index = 0;
        foreach (var jobSkill in jobSkills)
        {
            if (index >= count)
            {
                break;
            }

            skillLabels.Add($"{jobSkill.SkillName} (min {jobSkill.Score})");
            index++;
        }

        return skillLabels;
    }

    private static string GetFirstLine(string text)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return text;
        }

        return lines[0];
    }
}
