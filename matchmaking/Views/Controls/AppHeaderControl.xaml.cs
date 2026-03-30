using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace matchmaking.Views.Controls;

public sealed partial class AppHeaderControl : UserControl
{
    public event EventHandler? RecommendationsRequested;
    public event EventHandler? MyStatusRequested;
    public event EventHandler? ChatRequested;

    public AppHeaderControl()
    {
        InitializeComponent();
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
