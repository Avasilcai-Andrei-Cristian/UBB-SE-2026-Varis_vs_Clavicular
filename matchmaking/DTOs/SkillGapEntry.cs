namespace matchmaking.DTOs;

public class SkillGapEntry
{
    public string SkillName { get; set; } = string.Empty;
    public int UserScore { get; set; }
    public int RequiredScore { get; set; }
    public int JobCount { get; set; }

    public string GapText => $"Gap: {RequiredScore - UserScore} pts";
    public string UserScoreText => $"Your score: {UserScore}";
    public string RequiredScoreText => $"average required: {RequiredScore}";
    public string JobCountText => $"Required in {JobCount} rejected jobs";
}
