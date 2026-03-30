namespace matchmaking.Models;

public class UnderscoredSkillModel
{
    public string SkillName            { get; set; } = string.Empty;
    public int    UserScore            { get; set; }
    public int    AverageRequiredScore { get; set; }

    public string GapText                  => $"Gap: {AverageRequiredScore - UserScore} pts";
    public string UserScoreText            => $"Your score: {UserScore}";
    public string AverageRequiredScoreText => $"average required: {AverageRequiredScore}";
}
