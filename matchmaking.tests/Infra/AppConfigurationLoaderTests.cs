using System.Text.Json;
using matchmaking.Config;

namespace matchmaking.Tests;

public class AppConfigurationLoaderTests
{
    private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    [Fact]
    public void Load_valid_file_parses_connection_string_and_startup_settings()
    {
        lock (ConfigFileTestLock.Sync)
        {
            string? backup = File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : null;
            try
            {
                File.WriteAllText(ConfigPath, """
                    {
                        "ConnectionStrings": { "SqlServer": "Server=test;Database=db" },
                        "Startup": { "Mode": "company", "UserId": 5, "CompanyId": 3 },
                        "Recommendations": { "CooldownHours": 12 }
                    }
                    """);

                var config = AppConfigurationLoader.Load();

                config.SqlConnectionString.Should().Be("Server=test;Database=db");
                config.StartupMode.Should().Be("company");
                config.StartupUserId.Should().Be(5);
                config.StartupCompanyId.Should().Be(3);
                config.RecommendationCooldownHours.Should().Be(12);
            }
            finally
            {
                if (backup is not null)
                {
                    File.WriteAllText(ConfigPath, backup);
                }
                else if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }
            }
        }
    }

    [Fact]
    public void Load_missing_file_returns_default_configuration()
    {
        lock (ConfigFileTestLock.Sync)
        {
            string? backup = File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : null;
            try
            {
                if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }

                var config = AppConfigurationLoader.Load();

                config.SqlConnectionString.Should().BeEmpty();
                config.StartupMode.Should().Be("user");
                config.RecommendationCooldownHours.Should().Be(24);
            }
            finally
            {
                if (backup is not null)
                {
                    File.WriteAllText(ConfigPath, backup);
                }
            }
        }
    }

    [Fact]
    public void Load_malformed_json_throws_JsonException()
    {
        lock (ConfigFileTestLock.Sync)
        {
            string? backup = File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : null;
            try
            {
                File.WriteAllText(ConfigPath, "{ this is not valid json }");

                var act = () => AppConfigurationLoader.Load();

                act.Should().Throw<JsonException>();
            }
            finally
            {
                if (backup is not null)
                {
                    File.WriteAllText(ConfigPath, backup);
                }
                else if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }
            }
        }
    }
}
