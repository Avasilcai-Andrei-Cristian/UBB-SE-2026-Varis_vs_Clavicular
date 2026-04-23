using matchmaking.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace matchmaking.Views.Pages;

public sealed partial class CompanyProfilePage : Page
{
    public CompanyProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not int companyId || companyId <= 0)
        {
            NameText.Text = "Unknown company";
            ContactText.Text = string.Empty;
            JobsText.Text = string.Empty;
            return;
        }

        var company = new CompanyRepository().GetById(companyId);
        if (company is null)
        {
            NameText.Text = "Company not found";
            ContactText.Text = string.Empty;
            JobsText.Text = string.Empty;
            return;
        }

        NameText.Text = company.CompanyName;
        ContactText.Text = $"{company.Email} · {company.Phone}";
        var jobCount = new JobRepository().GetByCompanyId(companyId).Count;
        JobsText.Text = jobCount == 0
            ? "No jobs are seeded for this company yet."
            : $"{jobCount} job(s) available in the seeded dataset.";
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
