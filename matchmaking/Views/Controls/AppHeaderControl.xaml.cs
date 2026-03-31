using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using matchmaking.ViewModels;

namespace matchmaking.Views.Controls;

public sealed partial class AppHeaderControl : UserControl
{
    public event EventHandler? RecommendationsRequested;
    public event EventHandler? MyStatusRequested;
    public event EventHandler? ChatRequested;

    public AppHeaderControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is ShellViewModel vm)
        {
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ShellViewModel.ActivePage))
                    UpdateActiveButton(vm.ActivePage);
            };
            UpdateActiveButton(vm.ActivePage);
        }
    }

    private void UpdateActiveButton(string activePage)
    {
        var white       = new SolidColorBrush(Colors.White);
        var black       = new SolidColorBrush(Colors.Black);
        var transparent = new SolidColorBrush(Colors.Transparent);

        SetButtonState(RecommendationsButton, activePage == "Recommendations", white, black, transparent);
        SetButtonState(MyStatusButton,        activePage == "MyStatus",        white, black, transparent);
        SetButtonState(ChatButton,            activePage == "Chat",            white, black, transparent);
    }

    private static void SetButtonState(
        Button btn, bool isActive,
        SolidColorBrush white, SolidColorBrush black, SolidColorBrush transparent)
    {
        btn.Background  = isActive ? white       : transparent;
        btn.Foreground  = isActive ? black       : white;
        btn.FontWeight  = isActive
            ? Microsoft.UI.Text.FontWeights.SemiBold
            : Microsoft.UI.Text.FontWeights.Normal;
    }

    private void Recommendations_Click(object sender, RoutedEventArgs e)
    {
        RecommendationsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MyStatus_Click(object sender, RoutedEventArgs e)
    {
        MyStatusRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Chat_Click(object sender, RoutedEventArgs e)
    {
        ChatRequested?.Invoke(this, EventArgs.Empty);
    }
}
