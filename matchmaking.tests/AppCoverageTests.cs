using System.Reflection;
using matchmaking.Config;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;

namespace matchmaking.Tests;

[Collection("AppState")]
public sealed class AppCoverageTests
{
    [Fact]
    public void InitializeStartupSession_WhenStartupModeIsCompany_SetsCompanySession()
    {
        var previousConfiguration = GetAppConfiguration();
        var previousSession = GetAppSession();

        try
        {
            SetAppConfiguration(new AppConfiguration
            {
                StartupMode = "company",
                StartupCompanyId = 42,
                StartupUserId = 7,
                StartupDeveloperId = 9
            });

            InvokeInitializeStartupSession();

            GetAppSession().CurrentMode.Should().Be(AppMode.CompanyMode);
            GetAppSession().CurrentCompanyId.Should().Be(42);
        }
        finally
        {
            SetAppConfiguration(previousConfiguration);
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void InitializeStartupSession_WhenStartupModeIsInvalid_SetsUserSession()
    {
        var previousConfiguration = GetAppConfiguration();
        var previousSession = GetAppSession();

        try
        {
            SetAppConfiguration(new AppConfiguration
            {
                StartupMode = "unknown",
                StartupUserId = 17,
                StartupCompanyId = 18,
                StartupDeveloperId = 19
            });

            InvokeInitializeStartupSession();

            GetAppSession().CurrentMode.Should().Be(AppMode.UserMode);
            GetAppSession().CurrentUserId.Should().Be(17);
        }
        finally
        {
            SetAppConfiguration(previousConfiguration);
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void CheckDatabaseConnection_WhenConnectionStringIsMissing_ReturnsFalse()
    {
        var previousConfiguration = GetAppConfiguration();
        var previousAvailability = GetAppAvailability();
        var previousError = GetAppDatabaseError();

        try
        {
            SetAppConfiguration(new AppConfiguration());

            var result = App.CheckDatabaseConnection();

            result.Should().BeFalse();
            GetAppAvailability().Should().BeFalse();
            GetAppDatabaseError().Should().Be("Connection string is missing from appsettings.json.");
        }
        finally
        {
            SetAppConfiguration(previousConfiguration);
            SetAppAvailability(previousAvailability);
            SetAppDatabaseError(previousError);
        }
    }

    [Fact]
    public void CheckDatabaseConnection_WhenConnectionStringIsInvalid_ReturnsFalse()
    {
        var previousConfiguration = GetAppConfiguration();
        var previousAvailability = GetAppAvailability();
        var previousError = GetAppDatabaseError();

        try
        {
            SetAppConfiguration(new AppConfiguration
            {
                SqlConnectionString = "not-a-valid-connection-string"
            });

            var result = App.CheckDatabaseConnection();

            result.Should().BeFalse();
            GetAppAvailability().Should().BeFalse();
            GetAppDatabaseError().Should().NotBeEmpty();
        }
        finally
        {
            SetAppConfiguration(previousConfiguration);
            SetAppAvailability(previousAvailability);
            SetAppDatabaseError(previousError);
        }
    }

    private static void InvokeInitializeStartupSession()
    {
        typeof(App).GetMethod("InitializeStartupSession", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null);
    }

    private static AppConfiguration GetAppConfiguration()
    {
        return (AppConfiguration)typeof(App).GetProperty(nameof(App.Configuration), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
    }

    private static void SetAppConfiguration(AppConfiguration configuration)
    {
        typeof(App).GetProperty(nameof(App.Configuration), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, configuration);
    }

    private static SessionContext GetAppSession()
    {
        return (SessionContext)typeof(App).GetProperty(nameof(App.Session), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
    }

    private static void SetAppSession(SessionContext session)
    {
        typeof(App).GetProperty(nameof(App.Session), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, session);
    }

    private static bool GetAppAvailability()
    {
        return (bool)typeof(App).GetProperty(nameof(App.IsDatabaseConnectionAvailable), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
    }

    private static void SetAppAvailability(bool value)
    {
        typeof(App).GetProperty(nameof(App.IsDatabaseConnectionAvailable), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, value);
    }

    private static string GetAppDatabaseError()
    {
        return (string)typeof(App).GetProperty(nameof(App.DatabaseConnectionError), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
    }

    private static void SetAppDatabaseError(string value)
    {
        typeof(App).GetProperty(nameof(App.DatabaseConnectionError), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, value);
    }
}
