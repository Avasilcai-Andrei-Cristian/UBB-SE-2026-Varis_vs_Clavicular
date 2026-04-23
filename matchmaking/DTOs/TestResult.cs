using System.Collections.Generic;
using matchmaking.Domain.Enums;
using matchmaking.DTOs.TestingModule;

namespace matchmaking.DTOs;

public class TestResult
{
    public int MatchId { get; set; }
    public int UserId { get; set; }
    public int JobId { get; set; }
    public int ExternalUserId { get; set; }
    public int PositionId { get; set; }
    public MatchStatus Decision { get; set; }
    public string FeedbackMessage { get; set; } = string.Empty;

    public TestDefinitionRecord? Test { get; set; }
    public TestAttemptRecord? Attempt { get; set; }
    public InterviewSessionRecord? InterviewSession { get; set; }
    public IReadOnlyList<QuestionRecord> Questions { get; set; } = new List<QuestionRecord>();

    public bool IsValid { get; set; }
    public IReadOnlyList<string> ValidationErrors { get; set; } = new List<string>();
}
