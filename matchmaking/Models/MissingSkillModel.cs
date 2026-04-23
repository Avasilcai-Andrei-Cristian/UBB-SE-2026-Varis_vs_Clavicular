namespace matchmaking.Models;

public class MissingSkillModel
{
    public string SkillName { get; set; } = string.Empty;
    public int RejectedJobCount { get; set; }

    public string JobCountText => $"Required in {RejectedJobCount} rejected jobs";
}
