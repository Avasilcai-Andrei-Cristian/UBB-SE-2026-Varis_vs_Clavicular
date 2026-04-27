using System;

namespace matchmaking.Services
{
    public interface INavigationService
    {
        event Action<int>? CompanyProfileRequested;
        event Action<int>? JobPostRequested;
        event Action<int>? UserProfileRequested;

        void RequestCompanyProfile(int companyId);
        void RequestJobPost(int jobId);
        void RequestUserProfile(int userId);
    }
}