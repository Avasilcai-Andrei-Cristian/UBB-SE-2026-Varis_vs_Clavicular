using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using matchmaking.Config;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.algorithm;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace matchmaking
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public static AppConfiguration Configuration { get; private set; } = new();
        public static SessionContext Session { get; private set; } = new();
        public static bool IsDatabaseConnectionAvailable { get; private set; }
        public static string DatabaseConnectionError { get; private set; } = string.Empty;
        public static Window? MainWindow { get; private set; }
        public static NavigationService Navigation { get; } = new();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Configuration = AppConfigurationLoader.Load();
            Session = new SessionContext();

            InitializeStartupSession();

            CheckDatabaseConnection();
        }

        private static void InitializeStartupSession()
        {
            var startupMode = (Configuration.StartupMode ?? string.Empty).Trim().ToLowerInvariant();

            switch (startupMode)
            {
                case "company":
                    Session.LoginAsCompany(Configuration.StartupCompanyId);
                    break;
                case "developer":
                    Session.LoginAsDeveloper(Configuration.StartupDeveloperId);
                    break;
                default:
                    Session.LoginAsUser(Configuration.StartupUserId);
                    break;
            }

            if (Session.CurrentMode is not (AppMode.UserMode or AppMode.CompanyMode or AppMode.DeveloperMode))
            {
                Session.LoginAsUser(Configuration.StartupUserId);
            }
        }

        public static bool CheckDatabaseConnection()
        {
            if (string.IsNullOrWhiteSpace(Configuration.SqlConnectionString))
            {
                IsDatabaseConnectionAvailable = false;
                DatabaseConnectionError = "Connection string is missing from appsettings.json.";
                return false;
            }

            try
            {
                var ping = new SqlConnectionTestRepository(Configuration.SqlConnectionString).Ping();
                IsDatabaseConnectionAvailable = ping == 1;
                DatabaseConnectionError = IsDatabaseConnectionAvailable
                    ? string.Empty
                    : "Database ping returned an unexpected value.";
            }
            catch (Exception ex)
            {
                IsDatabaseConnectionAvailable = false;
                DatabaseConnectionError = ex.Message;
            }

            return IsDatabaseConnectionAvailable;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            MainWindow = _window;
            _window.Activate();
        }
    }
}
