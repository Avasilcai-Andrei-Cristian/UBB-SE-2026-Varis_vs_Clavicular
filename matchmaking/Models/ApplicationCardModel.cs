using System;
using matchmaking.Domain.Enums;

namespace matchmaking.Models;

public class ApplicationCardModel
{
    public int MatchId { get; set; }
    public int JobId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public MatchStatus Status { get; set; }
    public int CompatibilityScore { get; set; }
    public string FeedbackMessage { get; set; } = string.Empty;

    public string TruncatedDescription =>
        JobDescription.Length > 120 ? JobDescription[..120] + "..." : JobDescription;

    public string FormattedDate => $"Applied on {AppliedDate:dd MMM yyyy}";
    public string FormattedScore => $"{CompatibilityScore}% match";
    public bool HasFeedback => !string.IsNullOrEmpty(FeedbackMessage);
}
