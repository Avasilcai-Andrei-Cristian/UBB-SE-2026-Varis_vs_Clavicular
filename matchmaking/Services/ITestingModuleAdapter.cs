using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.DTOs;

namespace matchmaking.Services;

public interface ITestingModuleAdapter
{
    Task<TestResult?> GetResultForMatchAsync(int matchId);

    Task<TestResult?> GetLatestResultForCandidateAsync(int externalUserId, int positionId);

    Task<IReadOnlyList<TestResult>> GetResultHistoryForCandidateAsync(int externalUserId, int positionId);
}
