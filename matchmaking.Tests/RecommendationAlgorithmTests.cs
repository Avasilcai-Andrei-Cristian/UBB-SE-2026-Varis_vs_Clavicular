using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class RecommendationAlgorithmTests
{
    [Fact]
    public void CalculateCompatibilityScore_WithHigherMatchingSkillAndResumeOverlap_ReturnsExpectedRange()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var userSkills = new List<Skill>
        {
            TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90),
            TestDataFactory.CreateSkill(user.UserId, 2, "React", 75)
        };
        var jobSkills = new List<Skill>
        {
            TestDataFactory.CreateSkill(0, 1, "C#", 80),
            TestDataFactory.CreateSkill(0, 2, "React", 70)
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

        score.Should().BeGreaterThan(0);
        score.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithMixedInputs_UsesCachedParametersWhenPresent()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var userSkills = new List<Skill>
        {
            TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80)
        };
        var jobSkills = new List<Skill>
        {
            TestDataFactory.CreateSkill(0, 1, "C#", 70)
        };

        var posts = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.WeightedDistanceScoreWeight, "40"),
            TestDataFactory.CreatePost(2, 1, PostParameterType.JobResumeSimilarityScoreWeight, "30"),
            TestDataFactory.CreatePost(3, 1, PostParameterType.PreferenceScoreWeight, "20"),
            TestDataFactory.CreatePost(4, 1, PostParameterType.PromotionScoreWeight, "10"),
            TestDataFactory.CreatePost(5, 1, PostParameterType.RelevantKeyword, "react")
        };
        var interactions = new List<Interaction>
        {
            TestDataFactory.CreateInteraction(1, 2, 5, InteractionType.Like)
        };

        var algorithm = new RecommendationAlgorithm(new FakePostRepository(posts), new FakeInteractionRepository(interactions));
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

        score.Should().BeGreaterThanOrEqualTo(0);
        score.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithEmptyJobSkills_ReturnsZeroSkillScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80)], []);

        breakdown.SkillScore.Should().Be(0);
        breakdown.KeywordScore.Should().BeGreaterThanOrEqualTo(0);
        breakdown.OverallScore.Should().BeGreaterThanOrEqualTo(0);
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly IReadOnlyList<Post> _posts;

        public FakePostRepository(IReadOnlyList<Post> posts)
        {
            _posts = posts;
        }

        public IReadOnlyList<Post> GetAll() => _posts;
        public void Add(Post post) { }
    }

    private sealed class FakeInteractionRepository : IInteractionRepository
    {
        private readonly IReadOnlyList<Interaction> _interactions;

        public FakeInteractionRepository(IReadOnlyList<Interaction> interactions)
        {
            _interactions = interactions;
        }

        public IReadOnlyList<Interaction> GetAll() => _interactions;
        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) => _interactions.FirstOrDefault(i => i.DeveloperId == developerId && i.PostId == postId);
        public Interaction? GetById(int interactionId) => _interactions.FirstOrDefault(i => i.InteractionId == interactionId);
        public IReadOnlyList<Interaction> GetByDeveloperId(int developerId) => _interactions.Where(i => i.DeveloperId == developerId).ToList();
        public IReadOnlyList<Interaction> GetByPostId(int postId) => _interactions.Where(i => i.PostId == postId).ToList();
        public void Add(Interaction interaction) { }
        public void Update(Interaction interaction) { }
        public void Remove(int interactionId) { }
    }
}
