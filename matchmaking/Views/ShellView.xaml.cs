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
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ContentHostFrame.Content is null)
        {
            ContentHostFrame.Navigate(typeof(SampleFormPage));
        }
    }
}
