using System;
using System.Linq;

namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlRecommendationRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;
    public SqlRecommendationRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Add_persists_recommendation_and_assigns_generated_id()
    {
        var repository = new SqlRecommendationRepository(database.ConnectionString);
        const int userId = 1;
        const int jobId = 100;
        var recommendation = new Recommendation
        {
            UserId = userId,
            JobId = jobId,
            Timestamp = new DateTime(2025, 6, 1, 12, 0, 0)
        };
        repository.Add(recommendation);
        recommendation.RecommendationId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetById_retrieves_recommendation_that_was_persisted()
    {
        var repository = new SqlRecommendationRepository(database.ConnectionString);
        const int userId = 2;
        const int jobId = 200;
        var recommendation = new Recommendation
        {
            UserId = userId,
            JobId = jobId,
            Timestamp = new DateTime(2025, 7, 15, 9, 30, 0)
        };
        repository.Add(recommendation);
        var retrieved = repository.GetById(recommendation.RecommendationId);
        retrieved.Should().NotBeNull();
        retrieved!.UserId.Should().Be(userId);
        retrieved.JobId.Should().Be(jobId);
    }

    [Fact]
    public void GetLatestByUserIdAndJobId_returns_most_recently_added_recommendation()
    {
        var repository = new SqlRecommendationRepository(database.ConnectionString);
        const int userId = 3;
        const int jobId = 300;
        var olderRecommendation = new Recommendation
        {
            UserId = userId,
            JobId = jobId,
            Timestamp = new DateTime(2025, 1, 1, 8, 0, 0)
        };
        var newerRecommendation = new Recommendation
        {
            UserId = userId,
            JobId = jobId,
            Timestamp = new DateTime(2025, 12, 1, 8, 0, 0)
        };
        repository.Add(olderRecommendation);
        repository.Add(newerRecommendation);
        var latest = repository.GetLatestByUserIdAndJobId(userId, jobId);
        latest.Should().NotBeNull();
        latest!.RecommendationId.Should().Be(newerRecommendation.RecommendationId);
        latest.Timestamp.Should().Be(new DateTime(2025, 12, 1, 8, 0, 0));
    }
}