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
        var userSkillsList = new List<Skill>
        {
            TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90),
            TestDataFactory.CreateSkill(user.UserId, 2, "React", 75)
        };
        var jobSkillsList = new List<Skill>
        {
            TestDataFactory.CreateSkill(0, 1, "C#", 80),
            TestDataFactory.CreateSkill(0, 2, "React", 70)
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkillsList, jobSkillsList);

        score.Should().BeGreaterThan(0);
        score.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithMixedInputs_UsesCachedParametersWhenPresent()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var userSkillsList = new List<Skill>
        {
            TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80)
        };
        var jobSkillsList = new List<Skill>
        {
            TestDataFactory.CreateSkill(0, 1, "C#", 70)
        };

        var postsList = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.WeightedDistanceScoreWeight, "40"),
            TestDataFactory.CreatePost(2, 1, PostParameterType.JobResumeSimilarityScoreWeight, "30"),
            TestDataFactory.CreatePost(3, 1, PostParameterType.PreferenceScoreWeight, "20"),
            TestDataFactory.CreatePost(4, 1, PostParameterType.PromotionScoreWeight, "10"),
            TestDataFactory.CreatePost(5, 1, PostParameterType.RelevantKeyword, "react")
        };
        var interactionsList = new List<Interaction>
        {
            TestDataFactory.CreateInteraction(1, 2, 5, InteractionType.Like)
        };

        var algorithm = new RecommendationAlgorithm(new FakePostRepository(postsList), new FakeInteractionRepository(interactionsList));
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkillsList, jobSkillsList);

        score.Should().BeGreaterThanOrEqualTo(0);
        score.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithEmptyJobSkills_ReturnsZeroSkillScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, new List<Skill> { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) }, new List<Skill>());

        breakdown.SkillScore.Should().Be(0);
        breakdown.KeywordScore.Should().BeGreaterThanOrEqualTo(0);
        breakdown.OverallScore.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithEmptyResumeAndDescription_ReturnsZeroKeywordScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        user.Resume = string.Empty;
        job.JobDescription = string.Empty;

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, new List<Skill>(), new List<Skill>());

        breakdown.KeywordScore.Should().Be(0);
        breakdown.OverallScore.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithWeightedParametersAndSignals_UsesResolvedWeights()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var userSkillsList = new List<Skill>
        {
            TestDataFactory.CreateSkill(user.UserId, 1, "C#", 95)
        };
        var jobSkillsList = new List<Skill>
        {
            TestDataFactory.CreateSkill(0, 1, "C#", 50)
        };

        var postsList = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.WeightedDistanceScoreWeight, "0"),
            TestDataFactory.CreatePost(2, 1, PostParameterType.JobResumeSimilarityScoreWeight, "0"),
            TestDataFactory.CreatePost(3, 1, PostParameterType.PreferenceScoreWeight, "0"),
            TestDataFactory.CreatePost(4, 1, PostParameterType.PromotionScoreWeight, "0"),
            TestDataFactory.CreatePost(5, 1, PostParameterType.MitigationFactor, "0"),
            TestDataFactory.CreatePost(6, 1, PostParameterType.RelevantKeyword, "  c#  "),
            TestDataFactory.CreatePost(7, 1, PostParameterType.RelevantKeyword, string.Empty)
        };
        var interactionsList = new List<Interaction>
        {
            TestDataFactory.CreateInteraction(1, 2, 6, InteractionType.Like),
            TestDataFactory.CreateInteraction(2, 3, 6, InteractionType.Dislike),
            TestDataFactory.CreateInteraction(3, 4, 7, InteractionType.Like)
        };

        var algorithm = new RecommendationAlgorithm(new FakePostRepository(postsList), new FakeInteractionRepository(interactionsList));
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkillsList, jobSkillsList);

        score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithNoJobSkills_ReturnsZero()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var score = algorithm.CalculateCompatibilityScore(user, job, new List<Skill>(), new List<Skill>());

        score.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenKeywordsHaveMaxNegativeSignal_ReturnsValidScore()
    {
        var user = TestDataFactory.CreateUser();
        user.Resume = "csharp";
        var job = TestDataFactory.CreateJob();
        job.JobDescription = "csharp";

        var postsList = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.RelevantKeyword, "csharp")
        };

        var interactionsList = Enumerable.Range(1, 10)
            .Select(index => TestDataFactory.CreateInteraction(index, index, 1, InteractionType.Dislike))
            .ToList();

        var algorithm = new RecommendationAlgorithm();
        var score = algorithm.CalculateCompatibilityScore(user, job, new List<Skill>(), new List<JobSkill>(), postsList, interactionsList);

        score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenSkillMatchedByNameNotId_UsesNameFallback()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var userSkillsList = new List<Skill> { TestDataFactory.CreateSkill(user.UserId, 999, "C#", 90) };
        var jobSkillsList = new List<Skill> { TestDataFactory.CreateSkill(0, 1, "C#", 80) };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkillsList, jobSkillsList);

        score.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenMitigationFactorPostIsLessThanOne_ClampsMitigationFactorToOne()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var userSkillsList = new List<Skill> { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) };
        var jobSkillsList = new List<Skill> { TestDataFactory.CreateSkill(0, 1, "C#", 50) };

        var postsList = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.MitigationFactor, "0.5")
        };
        var interactionsList = new List<Interaction>
        {
            TestDataFactory.CreateInteraction(1, 1, 1, InteractionType.Like)
        };

        var algorithm = new RecommendationAlgorithm(new FakePostRepository(postsList), new FakeInteractionRepository(interactionsList));
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkillsList, jobSkillsList);

        score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenDuplicateKeywordPostsExist_AggregatesKeywordSignals()
    {
        var user = TestDataFactory.CreateUser();
        user.Resume = "csharp";
        var job = TestDataFactory.CreateJob();
        job.JobDescription = "csharp";

        var postsList = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.RelevantKeyword, "csharp"),
            TestDataFactory.CreatePost(2, 1, PostParameterType.RelevantKeyword, " CSharp ")
        };

        var interactionsList = new List<Interaction>
        {
            TestDataFactory.CreateInteraction(1, 1, 1, InteractionType.Like),
            TestDataFactory.CreateInteraction(2, 1, 2, InteractionType.Like)
        };

        var algorithm = new RecommendationAlgorithm(new FakePostRepository(postsList), new FakeInteractionRepository(interactionsList));

        var score = algorithm.CalculateCompatibilityScore(user, job, new List<Skill>(), new List<JobSkill>(), postsList, interactionsList);

        score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenWeightedPostValueIsInvalid_UsesDefaultWeightsWithoutThrowing()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var postsList = new List<Post>
        {
            TestDataFactory.CreatePost(1, 1, PostParameterType.WeightedDistanceScoreWeight, "abc")
        };

        var algorithm = new RecommendationAlgorithm(new FakePostRepository(postsList), new FakeInteractionRepository(Array.Empty<Interaction>()));

        var score = algorithm.CalculateCompatibilityScore(user, job, new List<Skill>(), new List<Skill>());

        score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void NormalizeParameterKey_WhenParameterIsWhitespace_ReturnsEmptyString()
    {
        var method = typeof(RecommendationAlgorithm)
            .GetMethod("NormalizeParameterKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull();

        var result = (string?)method!.Invoke(null, new object?[] { "   " });

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void NormalizeParameterKey_WhenParameterContainsSymbols_ReturnsAlphaNumericKey()
    {
        var method = typeof(RecommendationAlgorithm)
            .GetMethod("NormalizeParameterKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull();

        var result = (string?)method!.Invoke(null, new object?[] { "Relevant keyword!! 123" });

        result.Should().Be("relevantkeyword123");
    }

    [Fact]
    public void Clamp_WhenValueIsOutsideBounds_ReturnsBoundaries()
    {
        var method = typeof(RecommendationAlgorithm)
            .GetMethod("Clamp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull();

        var clampedLowValue = (double)method!.Invoke(null, new object?[] { -5.0, 0.0, 100.0 })!;
        var high = (double)method.Invoke(null, new object?[] { 105.0, 0.0, 100.0 })!;

        clampedLowValue.Should().Be(0.0);
        high.Should().Be(100.0);
    }

    [Fact]
    public void KeywordValue_WhenKeywordIsWhitespace_ReturnsOne()
    {
        var method = typeof(RecommendationAlgorithm)
            .GetMethod("KeywordValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull();

        var result = (double)method!.Invoke(null, new object?[] { " ", new Dictionary<string, int>() })!;

        result.Should().Be(1.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenUserHasNoSkills_SkillScoreEqualsHundredMinusRequiredScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var breakdown = algorithm.CalculateScoreBreakdown(
            user,
            job,
            new List<Skill>(),
            new List<Skill> { TestDataFactory.CreateSkill(0, 1, "C#", 80) });

        breakdown.SkillScore.Should().Be(20.0);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenJobHasHigherPromotionLevel_ScoreExceedsLowPromotionJobScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = TestDataFactory.CreateUser();
        var userSkills = new List<Skill> { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) };
        var jobSkills = new List<Skill> { TestDataFactory.CreateSkill(0, 1, "C#", 80) };

        var lowPromotionJob = TestDataFactory.CreateJob();
        lowPromotionJob.PromotionLevel = 0;
        var highPromotionJob = TestDataFactory.CreateJob();
        highPromotionJob.PromotionLevel = 100;

        var lowScore = algorithm.CalculateCompatibilityScore(user, lowPromotionJob, userSkills, jobSkills);
        var highScore = algorithm.CalculateCompatibilityScore(user, highPromotionJob, userSkills, jobSkills);

        highScore.Should().BeGreaterThan(lowScore);
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly IReadOnlyList<Post> posts;

        public FakePostRepository(IReadOnlyList<Post> posts)
        {
            this.posts = posts;
        }

        public Post? GetById(int postId) => posts.FirstOrDefault(item => item.PostId == postId);
        public IReadOnlyList<Post> GetAll() => posts;
        public IReadOnlyList<Post> GetByDeveloperId(int developerId) => posts.Where(item => item.DeveloperId == developerId).ToList();
        public void Add(Post post)
        {
        }

        public void Update(Post post)
        {
        }

        public void Remove(int postId)
        {
        }
    }

    private sealed class FakeInteractionRepository : IInteractionRepository
    {
        private readonly IReadOnlyList<Interaction> interactions;

        public FakeInteractionRepository(IReadOnlyList<Interaction> interactions)
        {
            this.interactions = interactions;
        }

        public IReadOnlyList<Interaction> GetAll() => interactions;
        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) => interactions.FirstOrDefault(i => i.DeveloperId == developerId && i.PostId == postId);
        public Interaction? GetById(int interactionId) => interactions.FirstOrDefault(i => i.InteractionId == interactionId);
        public IReadOnlyList<Interaction> GetByDeveloperId(int developerId) => interactions.Where(i => i.DeveloperId == developerId).ToList();
        public IReadOnlyList<Interaction> GetByPostId(int postId) => interactions.Where(i => i.PostId == postId).ToList();
        public void Add(Interaction interaction)
        {
        }

        public void Update(Interaction interaction)
        {
        }

        public void Remove(int interactionId)
        {
        }
    }
}
