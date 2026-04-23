using matchmaking.Repositories;

namespace matchmaking.ViewModels;

public sealed class CompanyProfileViewModel : ObservableObject
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobRepository _jobRepository;
    private string _name = string.Empty;
    private string _contact = string.Empty;
    private string _jobs = string.Empty;

    public CompanyProfileViewModel(ICompanyRepository companyRepository, IJobRepository jobRepository)
    {
        _companyRepository = companyRepository;
        _jobRepository = jobRepository;
    }

    public string Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public string Contact
    {
        get => _contact;
        private set => SetProperty(ref _contact, value);
    }

    public string Jobs
    {
        get => _jobs;
        private set => SetProperty(ref _jobs, value);
    }

    public void Load(int companyId)
    {
        if (companyId <= 0)
        {
            SetUnknownCompany();
            return;
        }

        var company = _companyRepository.GetById(companyId);
        if (company is null)
        {
            SetNotFoundCompany();
            return;
        }

        Name = company.CompanyName;
        Contact = $"{company.Email} · {company.Phone}";

        var jobCount = _jobRepository.GetByCompanyId(companyId).Count;
        Jobs = jobCount == 0
            ? "No jobs are seeded for this company yet."
            : $"{jobCount} job(s) available in the seeded dataset.";
    }

    private void SetUnknownCompany()
    {
        Name = "Unknown company";
        Contact = string.Empty;
        Jobs = string.Empty;
    }

    private void SetNotFoundCompany()
    {
        Name = "Company not found";
        Contact = string.Empty;
        Jobs = string.Empty;
    }
}
