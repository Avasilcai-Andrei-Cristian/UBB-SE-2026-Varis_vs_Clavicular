using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;

namespace matchmaking.Views.Pages;

public sealed partial class CompanyStatusPage : Page
{
    private readonly CompanyStatusViewModel _viewModel;

    public CompanyStatusPage()
    {
        InitializeComponent();

        var session = App.Session;

        var jobService = new JobService(new JobRepository());
        var matchService = new MatchService(
            new SqlMatchRepository(App.Configuration.SqlConnectionString),
            jobService);

        _viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                matchService,
                new UserService(new UserRepository()),
                jobService,
                new SkillService(new SkillRepository())),
            matchService,
            new TestingModuleAdapterStub(),
            session);

        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadApplicationsAsync();
    }
}
