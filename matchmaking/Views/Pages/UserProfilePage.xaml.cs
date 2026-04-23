using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using matchmaking.ViewModels;
using matchmaking.Repositories;

namespace matchmaking.Views.Pages;

public sealed partial class UserProfilePage : Page
{
    private readonly UserProfileViewModel _viewModel;

    public UserProfilePage()
    {
        InitializeComponent();
        _viewModel = new UserProfileViewModel(new UserRepository());
        DataContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _viewModel.Load(e.Parameter is int userId ? userId : 0);
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
