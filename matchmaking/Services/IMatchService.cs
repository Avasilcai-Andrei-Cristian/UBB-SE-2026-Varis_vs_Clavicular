using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Services
{
    public interface IMatchService
    {
        Task AcceptAsync(int matchId, string feedback);
        void Advance(int matchId);
        int CreatePendingApplication(int userId, int jobId);
        IReadOnlyList<Match> GetAllMatches();
        Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId);
        Match? GetById(int matchId);
        Match? GetByUserIdAndJobId(int userId, int jobId);
        bool IsDecisionTransitionAllowed(Match current, MatchStatus next);
        void Reject(int matchId, string feedback);
        Task RejectAsync(int matchId, string feedback);
        void RemoveApplication(int matchId);
        void RevertToApplied(int matchId);
        void SubmitDecision(int matchId, MatchStatus decision, string feedback);
        Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback);
    }
}