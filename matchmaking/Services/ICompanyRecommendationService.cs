using matchmaking.DTOs;

namespace matchmaking.Services
{
    public interface ICompanyRecommendationService
    {
        bool HasMore { get; }

        CompatibilityBreakdown? GetBreakdown(UserApplicationResult applicant);
        UserApplicationResult? GetNextApplicant();
        void LoadApplicants(int companyId);
        void MoveToNext();
        void MoveToPrevious();
    }
}