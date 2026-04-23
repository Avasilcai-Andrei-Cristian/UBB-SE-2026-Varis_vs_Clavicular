using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using matchmaking.ViewModels;
using matchmaking.Repositories;

namespace matchmaking.Views.Pages;

public sealed partial class CompanyProfilePage : Page
{
    private readonly CompanyProfileViewModel _viewModel;

    public CompanyProfilePage()
    {
        InitializeComponent();
        _viewModel = new CompanyProfileViewModel(new CompanyRepository(), new JobRepository());
        DataContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _viewModel.Load(e.Parameter is int companyId ? companyId : 0);
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
