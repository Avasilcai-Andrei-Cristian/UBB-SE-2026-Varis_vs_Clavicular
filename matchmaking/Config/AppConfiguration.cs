namespace matchmaking.Config;

public class AppConfiguration
{
    public string SqlConnectionString { get; set; } = string.Empty;
    public string StartupMode { get; set; } = "user";
    public int StartupUserId { get; set; } = 1;
    public int StartupCompanyId { get; set; } = 1;
    public int StartupDeveloperId { get; set; } = 1;
    public int RecommendationCooldownHours { get; set; } = 24;
}
