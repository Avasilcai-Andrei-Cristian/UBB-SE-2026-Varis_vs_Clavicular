using matchmaking.Domain.Session;

namespace matchmaking;

public sealed class AppStubConfiguration
{
    public string SqlConnectionString { get; set; } = string.Empty;
}

public static class App
{
    public static SessionContext Session { get; set; } = new SessionContext();

    public static AppStubConfiguration Configuration { get; set; } = new AppStubConfiguration();

    public static bool IsDatabaseConnectionAvailable { get; set; } = true;

    public static string DatabaseConnectionError { get; set; } = string.Empty;
}
