using System;

namespace matchmaking.DTOs.TestingModule;

public class TestDefinitionRecord
{
    public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class QuestionRecord
{
    public int QuestionId { get; set; }
    public int PositionId { get; set; }
    public int TestId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal QuestionScore { get; set; }
    public string QuestionAnswer { get; set; } = string.Empty;
}

public class TestAttemptRecord
{
    public int UserTestId { get; set; }
    public int TestId { get; set; }
    public int ExternalUserId { get; set; }
    public decimal Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string AnswersFilePath { get; set; } = string.Empty;
}

public class LeaderboardEntryRecord
{
    public int LeaderboardId { get; set; }
    public int TestId { get; set; }
    public int UserId { get; set; }
    public decimal NormalizedScore { get; set; }
    public int RankPosition { get; set; }
    public int TieBreakPriority { get; set; }
    public DateTime LastRecalculationAt { get; set; }
}

public class InterviewSessionRecord
{
    public int SessionId { get; set; }
    public int PositionId { get; set; }
    public int ExternalUserId { get; set; }
    public int InterviewerId { get; set; }
    public DateTime DateStart { get; set; }
    public string Video { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Score { get; set; }
}
