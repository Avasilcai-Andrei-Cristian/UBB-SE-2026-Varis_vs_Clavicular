using Microsoft.UI.Xaml;

namespace matchmaking;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        App.Session.LoginAsUser(1);
    }
}
