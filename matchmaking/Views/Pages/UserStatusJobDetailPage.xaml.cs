using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using matchmaking.Models;

namespace matchmaking.Views.Pages;

public sealed partial class UserStatusJobDetailPage : Page
{
    public UserStatusJobDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not UserStatusJobDetailPayload payload)
        {
            PageTitleText.Text = "Job Details";
            CompanyText.Text = string.Empty;
            ScoreText.Text = string.Empty;
            DescriptionText.Text = string.Empty;
            SkillList.ItemsSource = System.Array.Empty<string>();
            return;
        }

        PageTitleText.Text = "Job Details";
        CompanyText.Text = payload.Card.CompanyName;
        ScoreText.Text = payload.Card.FormattedScore;
        DescriptionText.Text = payload.Card.JobDescription;

        var skillLabels = new List<string>();
        foreach (var skill in payload.JobSkills)
        {
            skillLabels.Add($"- {skill.SkillName} minimum score: {skill.Score}");
        }

        SkillList.ItemsSource = skillLabels;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
            return;
        }

        Frame.Navigate(typeof(UserStatusPage));
    }
}
