using Microsoft.UI.Xaml.Controls;
using matchmaking.ViewModels;


namespace matchmaking.Views.Pages;

public sealed partial class SampleFormPage : Page
{
    public SampleFormPage()
    {
        InitializeComponent();
        DataContext = new SampleFormViewModel();
    }
}
