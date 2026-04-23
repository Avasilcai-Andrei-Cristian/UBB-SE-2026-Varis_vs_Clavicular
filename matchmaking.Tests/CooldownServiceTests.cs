using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class CooldownServiceTests
{
    [Fact]
    public void IsOnCooldown_WhenNoRecommendationExists_ReturnsFalse()
    {
        var repository = new FakeRecommendationRepository([]);
        var service = new CooldownService(repository, TimeSpan.FromHours(24));

        service.IsOnCooldown(1, 100, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsOnCooldown_WhenRecommendationIsRecent_ReturnsTrue()
    {
        var repository = new FakeRecommendationRepository([
            TestDataFactory.CreateRecommendation(1, 1, 100, DateTime.UtcNow.AddHours(-1))
        ]);
        var service = new CooldownService(repository, TimeSpan.FromHours(24));

        service.IsOnCooldown(1, 100, DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsOnCooldown_WhenRecommendationIsOld_ReturnsFalse()
    {
        var repository = new FakeRecommendationRepository([
            TestDataFactory.CreateRecommendation(1, 1, 100, DateTime.UtcNow.AddDays(-2))
        ]);
        var service = new CooldownService(repository, TimeSpan.FromHours(24));

        service.IsOnCooldown(1, 100, DateTime.UtcNow).Should().BeFalse();
    }

    private sealed class FakeRecommendationRepository : IRecommendationRepository
    {
        private readonly IReadOnlyList<Recommendation> _recommendations;

        public FakeRecommendationRepository(IReadOnlyList<Recommendation> recommendations)
        {
            _recommendations = recommendations;
        }

        public Recommendation? GetById(int recommendationId) => _recommendations.FirstOrDefault(r => r.RecommendationId == recommendationId);
        public IReadOnlyList<Recommendation> GetAll() => _recommendations;
        public void Add(Recommendation recommendation) { }
        public void Update(Recommendation recommendation) { }
        public void Remove(int recommendationId) { }
        public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) => _recommendations.Where(r => r.UserId == userId && r.JobId == jobId).OrderByDescending(r => r.Timestamp).FirstOrDefault();
        public int InsertReturningId(Recommendation recommendation) => 1;
    }
}
