using System;
using System.IO;
using System.Text.Json;

namespace matchmaking.Config;

public static class AppConfigurationLoader
{
    public static AppConfiguration Load()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(configPath))
        {
            return new AppConfiguration();
        }

        var json = File.ReadAllText(configPath);
        using var document = JsonDocument.Parse(json);

        var configuration = new AppConfiguration();
        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            && connectionStrings.TryGetProperty("SqlServer", out var sqlConnectionString))
        {
            configuration.SqlConnectionString = sqlConnectionString.GetString() ?? string.Empty;
        }

        if (document.RootElement.TryGetProperty("Startup", out var startup))
        {
            if (startup.TryGetProperty("Mode", out var mode))
            {
                configuration.StartupMode = mode.GetString() ?? configuration.StartupMode;
            }

            if (startup.TryGetProperty("UserId", out var userId) && userId.TryGetInt32(out var parsedUserId))
            {
                configuration.StartupUserId = parsedUserId;
            }

            if (startup.TryGetProperty("CompanyId", out var companyId) && companyId.TryGetInt32(out var parsedCompanyId))
            {
                configuration.StartupCompanyId = parsedCompanyId;
            }

            if (startup.TryGetProperty("DeveloperId", out var developerId) && developerId.TryGetInt32(out var parsedDeveloperId))
            {
                configuration.StartupDeveloperId = parsedDeveloperId;
            }
        }

        if (document.RootElement.TryGetProperty("Recommendations", out var recommendations)
            && recommendations.TryGetProperty("CooldownHours", out var cooldownHours)
            && cooldownHours.TryGetInt32(out var parsedCooldownHours))
        {
            configuration.RecommendationCooldownHours = parsedCooldownHours;
        }

        return configuration;
    }
}
