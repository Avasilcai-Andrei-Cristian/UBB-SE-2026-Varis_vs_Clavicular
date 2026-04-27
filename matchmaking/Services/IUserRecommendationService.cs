using matchmaking.DTOs;

namespace matchmaking.Services
{
    public interface IUserRecommendationService
    {
        static abstract string MapUserYearsToExperienceBucket(int yearsOfExperience);
        int ApplyDismiss(int userId, JobRecommendationResult card);
        int ApplyLike(int userId, JobRecommendationResult card);
        JobRecommendationResult? GetNextCard(int userId, UserMatchmakingFilters filters);
        JobRecommendationResult? RecalculateTopCardIgnoringCooldown(int userId, UserMatchmakingFilters filters);
        void UndoDismiss(int dismissRecommendationId, int? displayRecommendationId);
        void UndoLike(int matchId, int? displayRecommendationId);
    }
}