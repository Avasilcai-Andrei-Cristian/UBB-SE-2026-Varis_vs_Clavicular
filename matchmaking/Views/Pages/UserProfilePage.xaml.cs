using matchmaking.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace matchmaking.Views.Pages;

public sealed partial class UserProfilePage : Page
{
    public UserProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not int userId || userId <= 0)
        {
            NameText.Text = "Unknown user";
            MetaText.Text = string.Empty;
            ContactText.Text = string.Empty;
            ResumeText.Text = string.Empty;
            return;
        }

        var user = new UserRepository().GetById(userId);
        if (user is null)
        {
            NameText.Text = "User not found";
            MetaText.Text = string.Empty;
            ContactText.Text = string.Empty;
            ResumeText.Text = string.Empty;
            return;
        }

        NameText.Text = user.Name;
        MetaText.Text = $"{user.Location} · {user.YearsOfExperience} years · {user.Education}";
        ContactText.Text = $"{user.Email} · {user.Phone}";
        ResumeText.Text = string.IsNullOrWhiteSpace(user.Resume) ? "No resume provided." : user.Resume;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
