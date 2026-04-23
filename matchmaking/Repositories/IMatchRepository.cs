using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IMatchRepository
{
    Match? GetById(int matchId);
    IReadOnlyList<Match> GetAll();
    void Add(Match match);
    void Update(Match match);
    void Remove(int matchId);
    int InsertReturningId(Match match);
    Match? GetByUserIdAndJobId(int userId, int jobId);
}
