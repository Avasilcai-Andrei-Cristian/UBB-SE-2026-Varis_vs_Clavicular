using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.ViewModels;

namespace matchmaking.Views.Pages;

public sealed partial class SkillGapPage : Page
{
    private readonly SkillGapViewModel _vm;

    public SkillGapPage()
    {
        InitializeComponent();

        _vm         = new SkillGapViewModel();
        DataContext = _vm;

        Loaded += (_, _) => _ = _vm.LoadData();
    }

    private void BackToStatus_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
            Frame.GoBack();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
        => _vm.Refresh();
}
