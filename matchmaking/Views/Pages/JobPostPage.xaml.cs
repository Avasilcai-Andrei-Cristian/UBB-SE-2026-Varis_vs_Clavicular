using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using matchmaking.ViewModels;
using matchmaking.Repositories;

namespace matchmaking.Views.Pages;

public sealed partial class JobPostPage : Page
{
    private readonly JobPostViewModel _viewModel;

    public JobPostPage()
    {
        InitializeComponent();
        _viewModel = new JobPostViewModel(new JobRepository());
        DataContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _viewModel.Load(e.Parameter is int jobId ? jobId : 0);
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
