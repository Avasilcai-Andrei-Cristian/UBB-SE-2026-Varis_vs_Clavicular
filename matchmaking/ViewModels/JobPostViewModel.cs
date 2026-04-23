using matchmaking.Repositories;

namespace matchmaking.ViewModels;

public sealed class JobPostViewModel : ObservableObject
{
    private readonly IJobRepository _jobRepository;
    private string _title = string.Empty;
    private string _meta = string.Empty;
    private string _description = string.Empty;

    public JobPostViewModel(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public string Meta
    {
        get => _meta;
        private set => SetProperty(ref _meta, value);
    }

    public string Description
    {
        get => _description;
        private set => SetProperty(ref _description, value);
    }

    public void Load(int jobId)
    {
        if (jobId <= 0)
        {
            SetUnknownJob();
            return;
        }

        var job = _jobRepository.GetById(jobId);
        if (job is null)
        {
            SetNotFoundJob();
            return;
        }

        Title = string.IsNullOrWhiteSpace(job.JobTitle) ? "Untitled Job" : job.JobTitle;
        Meta = $"{job.Location} · {job.EmploymentType}";
        Description = string.IsNullOrWhiteSpace(job.JobDescription)
            ? "No job description provided."
            : job.JobDescription;
    }

    private void SetUnknownJob()
    {
        Title = "Unknown job";
        Meta = string.Empty;
        Description = string.Empty;
    }

    private void SetNotFoundJob()
    {
        Title = "Job not found";
        Meta = string.Empty;
        Description = string.Empty;
    }
}
