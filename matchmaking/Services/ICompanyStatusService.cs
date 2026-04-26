using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.DTOs;

namespace matchmaking.Services
{
    public interface ICompanyStatusService
    {
        Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId);
        Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId);
    }
}