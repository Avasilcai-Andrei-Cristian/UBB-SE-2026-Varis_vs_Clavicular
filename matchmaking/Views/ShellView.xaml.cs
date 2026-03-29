using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.ViewModels;
using matchmaking.Views.Pages;

namespace matchmaking.Views;

public sealed partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
        DataContext = new ShellViewModel();

        HeaderControl.RecommendationsRequested += OnRecommendationsRequested;
        HeaderControl.MyStatusRequested += OnMyStatusRequested;
        HeaderControl.ChatRequested += OnChatRequested;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ContentHostFrame.Content is null)
        {
            ContentHostFrame.Navigate(typeof(ChatPageView));
        }
    }

    private void OnRecommendationsRequested(object? sender, System.EventArgs e)
    {
        ContentHostFrame.Navigate(typeof(SampleFormPage));
    }

    private void OnMyStatusRequested(object? sender, System.EventArgs e)
    {
        ContentHostFrame.Navigate(typeof(SampleFormPage));
    }

    private void OnChatRequested(object? sender, System.EventArgs e)
    {
        ContentHostFrame.Navigate(typeof(ChatPageView));
    }
}
