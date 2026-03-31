using matchmaking.Services;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace matchmaking.Views.Pages;

public sealed partial class UserRecommendationPageView : Page
{
    public UserRecommendationViewModel ViewModel { get; }

    public UserRecommendationPageView()
    {
        InitializeComponent();
        ViewModel = new UserRecommendationViewModel(
            MatchmakingComposition.CreateUserRecommendationService(App.Configuration.SqlConnectionString),
            App.Session);
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.InitializeAsync();
    }
}
