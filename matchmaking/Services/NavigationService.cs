using System;

namespace matchmaking.Services;

public sealed class NavigationService : INavigationService
{
    public event Action<int>? UserProfileRequested;
    public event Action<int>? CompanyProfileRequested;
    public event Action<int>? JobPostRequested;

    public void RequestUserProfile(int userId)
    {
        UserProfileRequested?.Invoke(userId);
    }

    public void RequestCompanyProfile(int companyId)
    {
        CompanyProfileRequested?.Invoke(companyId);
    }

    public void RequestJobPost(int jobId)
    {
        JobPostRequested?.Invoke(jobId);
    }
}
