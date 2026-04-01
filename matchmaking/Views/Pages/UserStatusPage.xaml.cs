using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using matchmaking.Models;
using matchmaking.ViewModels;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
namespace matchmaking.Views.Pages;

public sealed partial class UserStatusPage : Page
{
    private readonly UserStatusViewModel _vm;

    public UserStatusPage()
    {
        InitializeComponent();

        _vm         = new UserStatusViewModel();
        DataContext = _vm;

        Loaded += (_, _) =>
        {
            SetActiveFilter(FilterAll);
            _ = _vm.LoadMatches();
        };
    }

    // ── Filter ────────────────────────────────────────────────────────────────

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        SetActiveFilter(btn);
        _vm.ApplyFilter(btn.Tag?.ToString() ?? "All");
    }

    private void SetActiveFilter(Button activeBtn)
    {
        foreach (var btn in new[] { FilterAll, FilterApplied, FilterAccepted, FilterRejected })
        {
            if (btn == activeBtn)
            {
                btn.Background  = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
                btn.Foreground  = new SolidColorBrush(Colors.White);
                btn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
            }
            else
            {
                btn.Background  = new SolidColorBrush(Colors.White);
                btn.Foreground  = new SolidColorBrush(Colors.Black);
                btn.BorderBrush = new SolidColorBrush(Colors.Black);
            }
        }
    }

    // ── Card button handlers ──────────────────────────────────────────────────

    private async void ViewJobDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ApplicationCardModel model })
            await ShowJobDetailsAsync(model);
    }

    private void ViewSkillGap_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SkillGapPage));

    // ── Top bar handlers ──────────────────────────────────────────────────────

    private void SkillInsightsButton_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SkillGapPage));

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
        => _vm.Refresh();

    // ── Job Details Dialog ────────────────────────────────────────────────────

    private async Task ShowJobDetailsAsync(ApplicationCardModel model)
    {
        var jobSkills = _vm.GetJobSkills(model.JobId);

        ContentDialog? dialog = null;

        var shortTitle = model.JobDescription.Length > 50
            ? model.JobDescription[..50] + "..."
            : model.JobDescription;

        var titleGrid = new Grid { MinWidth = 380 };
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleText = new TextBlock
        {
            Text              = shortTitle,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping      = TextWrapping.Wrap
        };
        Grid.SetColumn(titleText, 0);

        var closeBtn = new Button
        {
            Content           = "✕",
            Padding           = new Thickness(10, 4, 10, 4),
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(12, 0, 0, 0)
        };
        closeBtn.Click += (_, _) => dialog?.Hide();
        Grid.SetColumn(closeBtn, 1);

        titleGrid.Children.Add(titleText);
        titleGrid.Children.Add(closeBtn);

        var content = new StackPanel { Spacing = 8, Width = 380 };

        content.Children.Add(new TextBlock
        {
            Text       = model.CompanyName,
            FontWeight = FontWeights.SemiBold,
            FontSize   = 14
        });

        content.Children.Add(new TextBlock
        {
            Text         = model.JobDescription,
            TextWrapping = TextWrapping.Wrap,
            FontSize     = 13
        });

        content.Children.Add(new TextBlock
        {
            Text       = $"{model.CompatibilityScore}% match",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 21, 101, 192)),
            FontWeight = FontWeights.SemiBold
        });

        if (jobSkills.Count > 0)
        {
            content.Children.Add(new TextBlock
            {
                Text       = "Required Skills:",
                FontWeight = FontWeights.SemiBold,
                Margin     = new Thickness(0, 8, 0, 0)
            });

            foreach (var skill in jobSkills)
            {
                content.Children.Add(new TextBlock
                {
                    Text     = $"• {skill.SkillName}  —  min score: {skill.Score}",
                    FontSize = 13
                });
            }
        }

        dialog = new ContentDialog
        {
            Title    = titleGrid,
            Content  = new ScrollViewer { Content = content, MaxHeight = 420 },
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync().AsTask();
    }
}
