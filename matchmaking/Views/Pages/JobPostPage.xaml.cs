using matchmaking.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace matchmaking.Views.Pages;

public sealed partial class JobPostPage : Page
{
    public JobPostPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not int jobId || jobId <= 0)
        {
            TitleText.Text = "Unknown job";
            MetaText.Text = string.Empty;
            DescriptionText.Text = string.Empty;
            return;
        }

        var job = new JobRepository().GetById(jobId);
        if (job is null)
        {
            TitleText.Text = "Job not found";
            MetaText.Text = string.Empty;
            DescriptionText.Text = string.Empty;
            return;
        }

        TitleText.Text = string.IsNullOrWhiteSpace(job.JobTitle) ? "Untitled Job" : job.JobTitle;
        MetaText.Text = $"{job.Location} · {job.EmploymentType}";
        DescriptionText.Text = string.IsNullOrWhiteSpace(job.JobDescription)
            ? "No job description provided."
            : job.JobDescription;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
