using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IRecommendationRepository
{
    Recommendation? GetById(int recommendationId);
    IReadOnlyList<Recommendation> GetAll();
    void Add(Recommendation recommendation);
    void Update(Recommendation recommendation);
    void Remove(int recommendationId);
    Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId);
    int InsertReturningId(Recommendation recommendation);
}
