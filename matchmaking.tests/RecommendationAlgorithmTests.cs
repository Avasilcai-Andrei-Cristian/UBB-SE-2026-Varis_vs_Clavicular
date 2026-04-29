using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class RecommendationAlgorithmTests
{
    private const double ExactPrecision = 0.000001;
    private const double RoundedPrecision = 0.05;

    [Fact]
    public void CalculateCompatibilityScore_WithDefaultConstructor_UsesEqualComponentWeights()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "developer", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "manager", location: "Office", employmentType: "Full-time", promotionLevel: 100);
        var score = algorithm.CalculateCompatibilityScore(user, job, Array.Empty<Skill>(), Array.Empty<Skill>());
        score.Should().BeApproximately(25.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithDefaultConstructor_UsesDefaultMitigationFactorForExtraSkillScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "developer", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "manager", location: "Office", employmentType: "Full-time", promotionLevel: 0);
        var userSkills = new[] { Skill(1, "C#", 90) };
        var jobSkills = new[] { Skill(1, "C#", 50) };
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);
        score.Should().BeApproximately(20.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithDefaultConstructor_UsesDefaultKeywordValuesWhenNoSignalsExist()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "alpha", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "alpha beta", location: "Office", employmentType: "Full-time", promotionLevel: 0);
        var score = algorithm.CalculateCompatibilityScore(user, job, Array.Empty<Skill>(), Array.Empty<Skill>());
        score.Should().BeApproximately(12.5, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithAllDefaultComponentsAtMaximum_ReturnsOneHundred()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "alpha beta", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "alpha beta", location: "Remote", employmentType: "Contract", promotionLevel: 150);
        var userSkills = new[] { Skill(1, "C#", 80) };
        var jobSkills = new[] { Skill(1, "C#", 80) };
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);
        score.Should().BeApproximately(100.0, ExactPrecision);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDefaultConstructor_ReturnsRoundedComponentValues()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "alpha beta", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "alpha gamma", location: "Remote", employmentType: "Contract", promotionLevel: 20);
        var userSkills = new[] { Skill(1, "C#", 90) };
        var jobSkills = new[] { Skill(1, "C#", 80) };
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);
        breakdown.SkillScore.Should().Be(95.0);
        breakdown.KeywordScore.Should().Be(33.3);
        breakdown.PreferenceScore.Should().Be(100.0);
        breakdown.PromotionScore.Should().Be(20.0);
        breakdown.OverallScore.Should().Be(62.1);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithLikedWeightPosts_ResolvesExpectedWeights()
    {
        var algorithm = AlgorithmFrom(
            WeightPosts(skill: 40, resume: 30, preference: 20, promotion: 10),
            new[] { Like(1), Like(2), Like(3), Like(4) });
        var skillScenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.WeightedDistanceScoreWeight);
        var resumeScenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.JobResumeSimilarityScoreWeight);
        var preferenceScenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.PreferenceScoreWeight);
        var promotionScenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.PromotionScoreWeight);
        var skillScore = algorithm.CalculateCompatibilityScore(
            skillScenario.User,
            skillScenario.Job,
            skillScenario.UserSkills,
            skillScenario.JobSkills);
        var resumeScore = algorithm.CalculateCompatibilityScore(
            resumeScenario.User,
            resumeScenario.Job,
            resumeScenario.UserSkills,
            resumeScenario.JobSkills);
        var preferenceScore = algorithm.CalculateCompatibilityScore(
            preferenceScenario.User,
            preferenceScenario.Job,
            preferenceScenario.UserSkills,
            preferenceScenario.JobSkills);
        var promotionScore = algorithm.CalculateCompatibilityScore(
            promotionScenario.User,
            promotionScenario.Job,
            promotionScenario.UserSkills,
            promotionScenario.JobSkills);
        skillScore.Should().BeApproximately(40.0, ExactPrecision);
        resumeScore.Should().BeApproximately(30.0, ExactPrecision);
        preferenceScore.Should().BeApproximately(20.0, ExactPrecision);
        promotionScore.Should().BeApproximately(10.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithDislikedWeightPost_GivesThatProposedWeightNoVoteValue()
    {
        var algorithm = AlgorithmFrom(
            new[] { Post(1, PostParameterType.WeightedDistanceScoreWeight, "80") },
            new[] { Dislike(1) });
        var scenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.WeightedDistanceScoreWeight);
        var score = algorithm.CalculateCompatibilityScore(
            scenario.User,
            scenario.Job,
            scenario.UserSkills,
            scenario.JobSkills);
        score.Should().BeApproximately(0.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithMoreLikesForOneWeightPost_IncreasesThatComponentShare()
    {
        var algorithm = AlgorithmFrom(
            WeightPosts(skill: 10, resume: 10, preference: 10, promotion: 10),
            new[] { Like(1), Like(2), Like(3), Like(3, developerId: 4), Like(3, developerId: 5), Like(3, developerId: 6), Like(4) });
        var scenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.PreferenceScoreWeight);
        var score = algorithm.CalculateCompatibilityScore(
            scenario.User,
            scenario.Job,
            scenario.UserSkills,
            scenario.JobSkills);
        score.Should().BeApproximately(100.0 * 40.0 / 70.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithEmptyDynamicPosts_UsesDefaultWeights()
    {
        var algorithm = AlgorithmFrom(Array.Empty<Post>(), Array.Empty<Interaction>());
        var scenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.PromotionScoreWeight);
        var score = algorithm.CalculateCompatibilityScore(
            scenario.User,
            scenario.Job,
            scenario.UserSkills,
            scenario.JobSkills);
        score.Should().BeApproximately(25.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithZeroDynamicWeightSum_FallsBackToDefaultWeights()
    {
        var algorithm = AlgorithmFrom(
            WeightPosts(skill: 0, resume: 0, preference: 0, promotion: 0),
            new[] { Like(1), Like(2), Like(3), Like(4) });
        var scenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.JobResumeSimilarityScoreWeight);
        var score = algorithm.CalculateCompatibilityScore(
            scenario.User,
            scenario.Job,
            scenario.UserSkills,
            scenario.JobSkills);
        score.Should().BeApproximately(25.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithInvalidDynamicWeightValue_IgnoresThatPostAndUsesDefaultWeight()
    {
        var algorithm = AlgorithmFrom(
            new[] { Post(1, PostParameterType.WeightedDistanceScoreWeight, "not-a-number") },
            new[] { Like(1) });
        var scenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.WeightedDistanceScoreWeight);
        var score = algorithm.CalculateCompatibilityScore(
            scenario.User,
            scenario.Job,
            scenario.UserSkills,
            scenario.JobSkills);
        score.Should().BeApproximately(25.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WhenMitigationFactorPostIsLessThanOne_ClampsMitigationFactorToOne()
    {
        var algorithm = AlgorithmFrom(
            new[] { Post(1, PostParameterType.MitigationFactor, "0.5") },
            new[] { Like(1) });
        var user = User(resume: "developer", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "manager", location: "Office", employmentType: "Full-time", promotionLevel: 0);
        var userSkills = new[] { Skill(1, "C#", 90) };
        var jobSkills = new[] { Skill(1, "C#", 50) };
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);
        score.Should().BeApproximately(15.0, ExactPrecision);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithManyVotes_WeightsPostValuesByPositiveVoteDelta()
    {
        var algorithm = AlgorithmFrom(
            WeightPosts(skill: 20, resume: 20, preference: 20, promotion: 20),
            new[] { Like(1), Like(2), Like(2, developerId: 3), Like(2, developerId: 4), Like(3), Like(4) });
        var scenario = ScenarioWithOnlyComponentAtOneHundred(PostParameterType.JobResumeSimilarityScoreWeight);
        var score = algorithm.CalculateCompatibilityScore(
            scenario.User,
            scenario.Job,
            scenario.UserSkills,
            scenario.JobSkills);
        score.Should().BeApproximately(50.0, ExactPrecision);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenAllRequiredSkillsAreMatched_ReturnsFullSkillScore()
    {
        var breakdown = CalculateBreakdown(
            userSkills: new[] { Skill(1, "C#", 80), Skill(2, "SQL", 60) },
            jobSkills: new[] { Skill(1, "C#", 80), Skill(2, "SQL", 60) });
        breakdown.SkillScore.Should().Be(100.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenNoRequiredSkillIsMatched_ReturnsPenaltyFromRequiredSkillScore()
    {
        var breakdown = CalculateBreakdown(
            userSkills: new[] { Skill(99, "Java", 80) },
            jobSkills: new[] { Skill(1, "C#", 80) });
        breakdown.SkillScore.Should().Be(20.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenSomeRequiredSkillsAreMissing_AveragesSkillPenalties()
    {
        var breakdown = CalculateBreakdown(
            userSkills: new[] { Skill(1, "C#", 80) },
            jobSkills: new[] { Skill(1, "C#", 80), Skill(2, "SQL", 60) });
        breakdown.SkillScore.Should().Be(70.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDuplicateDeveloperSkills_UsesTheLatestScoreForThatSkill()
    {
        var breakdown = CalculateBreakdown(
            userSkills: new[] { Skill(1, "C#", 40), Skill(1, "C#", 90) },
            jobSkills: new[] { Skill(1, "C#", 80) });
        breakdown.SkillScore.Should().Be(95.0);
    }

    [Fact]
    public void CalculateCompatibilityScore_WithJobSkillRows_UsesJobSkillScoreAsRequiredScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "developer", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "manager", location: "Office", employmentType: "Full-time", promotionLevel: 0);
        var userSkills = new[] { Skill(1, "C#", 60) };
        var jobSkills = new[] { JobSkill(1, "C#", 90) };
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills, Array.Empty<Post>(), Array.Empty<Interaction>());
        score.Should().BeApproximately(17.5, ExactPrecision);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenDeveloperHasNoSkills_PenalizesEveryRequiredSkill()
    {
        var breakdown = CalculateBreakdown(
            userSkills: Array.Empty<Skill>(),
            jobSkills: new[] { Skill(1, "C#", 80) });
        breakdown.SkillScore.Should().Be(20.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithIdenticalResumeAndJobDescription_ReturnsFullKeywordScore()
    {
        var breakdown = CalculateBreakdown(resume: "alpha beta", jobDescription: "alpha beta");
        breakdown.KeywordScore.Should().Be(100.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDisjointResumeAndJobDescription_ReturnsZeroKeywordScore()
    {
        var breakdown = CalculateBreakdown(resume: "alpha beta", jobDescription: "gamma delta");
        breakdown.KeywordScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDifferentTextCasing_NormalizesKeywordTokens()
    {
        var breakdown = CalculateBreakdown(resume: "React SQL", jobDescription: "react sql");
        breakdown.KeywordScore.Should().Be(100.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithEmptyResume_ReturnsZeroKeywordScore()
    {
        var breakdown = CalculateBreakdown(resume: string.Empty, jobDescription: "alpha");
        breakdown.KeywordScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithEmptyJobDescription_ReturnsZeroKeywordScore()
    {
        var breakdown = CalculateBreakdown(resume: "alpha", jobDescription: string.Empty);
        breakdown.KeywordScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithRepeatedResumeAndJobTokens_CountsEachTokenOnce()
    {
        var breakdown = CalculateBreakdown(resume: "alpha alpha beta", jobDescription: "alpha gamma gamma");
        breakdown.KeywordScore.Should().Be(33.3);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenOnlyLocationPreferenceMatches_ReturnsHalfPreferenceScore()
    {
        var breakdown = CalculateBreakdown(
            preferredLocation: "Cluj",
            preferredEmploymentType: "Contract",
            jobLocation: "Cluj",
            jobEmploymentType: "Full-time");
        breakdown.PreferenceScore.Should().Be(50.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenOnlyEmploymentPreferenceMatches_ReturnsHalfPreferenceScore()
    {
        var breakdown = CalculateBreakdown(
            preferredLocation: "Remote",
            preferredEmploymentType: "Full-time",
            jobLocation: "Cluj",
            jobEmploymentType: "Full-time");
        breakdown.PreferenceScore.Should().Be(50.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenBothPreferencesMatch_ReturnsFullPreferenceScore()
    {
        var breakdown = CalculateBreakdown(
            preferredLocation: "Cluj",
            preferredEmploymentType: "Full-time",
            jobLocation: "Cluj",
            jobEmploymentType: "Full-time");
        breakdown.PreferenceScore.Should().Be(100.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenNoPreferencesMatch_ReturnsZeroPreferenceScore()
    {
        var breakdown = CalculateBreakdown(
            preferredLocation: "Remote",
            preferredEmploymentType: "Contract",
            jobLocation: "Cluj",
            jobEmploymentType: "Full-time");
        breakdown.PreferenceScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenUserPreferenceFiltersAreNull_ReturnsZeroPreferenceScore()
    {
        var breakdown = CalculateBreakdown(
            preferredLocation: null!,
            preferredEmploymentType: null!,
            jobLocation: "Cluj",
            jobEmploymentType: "Full-time");
        breakdown.PreferenceScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenPromotionLevelIsBelowZero_ClampsPromotionScoreToZero()
    {
        var breakdown = CalculateBreakdown(promotionLevel: -25);
        breakdown.PromotionScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenPromotionLevelIsInsideBounds_UsesPromotionLevelAsScore()
    {
        var breakdown = CalculateBreakdown(promotionLevel: 42);
        breakdown.PromotionScore.Should().Be(42.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WhenPromotionLevelIsAboveOneHundred_ClampsPromotionScoreToOneHundred()
    {
        var breakdown = CalculateBreakdown(promotionLevel: 130);
        breakdown.PromotionScore.Should().Be(100.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithVeryPositiveKeywordSignal_BoundsKeywordValueAtMaximum()
    {
        var breakdown = CalculateBreakdown(
            resume: "react",
            jobDescription: "react java",
            posts: new[] { Post(1, PostParameterType.RelevantKeyword, "react") },
            interactions: LikesForPost(postId: 1, count: 100));
        breakdown.KeywordScore.Should().Be(83.3);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithPositiveKeywordSignal_ScalesKeywordValueBySignal()
    {
        var breakdown = CalculateBreakdown(
            resume: "react",
            jobDescription: "react java",
            posts: new[] { Post(1, PostParameterType.RelevantKeyword, "react") },
            interactions: new[] { Like(1), Like(1, developerId: 2) });
        breakdown.KeywordScore.Should().Be(54.5);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithMissingKeywordSignal_UsesDefaultKeywordValue()
    {
        var breakdown = CalculateBreakdown(
            resume: "react",
            jobDescription: "react java",
            posts: Array.Empty<Post>(),
            interactions: Array.Empty<Interaction>());
        breakdown.KeywordScore.Should().Be(50.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithAccentedAndUnaccentedKeywords_TreatsThemAsDifferentOrdinalTokens()
    {
        var breakdown = CalculateBreakdown(resume: "cafe", jobDescription: "caf\u00e9");
        breakdown.KeywordScore.Should().Be(0.0);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDuplicateKeywordPosts_AggregatesKeywordSignals()
    {
        var breakdown = CalculateBreakdown(
            resume: "react",
            jobDescription: "react java",
            posts: new[]
            {
                Post(1, PostParameterType.RelevantKeyword, "react"),
                Post(2, PostParameterType.RelevantKeyword, " React ")
            },
            interactions: new[] { Like(1), Like(2), Like(2, developerId: 2) });
        breakdown.KeywordScore.Should().Be(56.5);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDefaultConstructor_OverallScoreMatchesCalculatedScoreRoundedToOneDecimal()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: "alpha beta", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "alpha gamma", location: "Remote", employmentType: "Contract", promotionLevel: 20);
        var userSkills = new[] { Skill(1, "C#", 90) };
        var jobSkills = new[] { Skill(1, "C#", 80) };
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);
        breakdown.OverallScore.Should().Be(Math.Round(score, 1));
        DefaultWeightedTotal(breakdown).Should().BeApproximately(breakdown.OverallScore, RoundedPrecision);
    }

    [Fact]
    public void CalculateScoreBreakdown_WithDynamicConstructor_OverallScoreMatchesDynamicWeightedScore()
    {
        var algorithm = AlgorithmFrom(
            WeightPosts(skill: 40, resume: 30, preference: 20, promotion: 10),
            new[] { Like(1), Like(2), Like(3), Like(4) });
        var user = User(resume: "alpha", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "alpha beta", location: "Remote", employmentType: "Contract", promotionLevel: 10);
        var userSkills = new[] { Skill(1, "C#", 80) };
        var jobSkills = new[] { Skill(1, "C#", 80) };
        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);
        score.Should().BeApproximately(76.0, ExactPrecision);
        breakdown.OverallScore.Should().Be(76.0);
        breakdown.OverallScore.Should().Be(Math.Round(score, 1));
    }

    [Fact]
    public void CalculateScoreBreakdown_WithAllComponentsAtMinimum_OverallScoreMatchesZeroCalculatedScore()
    {
        var algorithm = new RecommendationAlgorithm();
        var user = User(resume: string.Empty, preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: string.Empty, location: "Office", employmentType: "Full-time", promotionLevel: -10);
        var score = algorithm.CalculateCompatibilityScore(user, job, Array.Empty<Skill>(), Array.Empty<Skill>());
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, Array.Empty<Skill>(), Array.Empty<Skill>());
        score.Should().BeApproximately(0.0, ExactPrecision);
        breakdown.OverallScore.Should().Be(0.0);
    }

    private static CompatibilityBreakdown CalculateBreakdown(
        IReadOnlyList<Skill>? userSkills = null,
        IReadOnlyList<Skill>? jobSkills = null,
        string resume = "resume",
        string jobDescription = "job",
        string preferredLocation = "Cluj",
        string preferredEmploymentType = "Full-time",
        string jobLocation = "Cluj",
        string jobEmploymentType = "Full-time",
        int promotionLevel = 0,
        IReadOnlyList<Post>? posts = null,
        IReadOnlyList<Interaction>? interactions = null)
    {
        var resolvedPosts = posts ?? Array.Empty<Post>();
        var resolvedInteractions = interactions ?? Array.Empty<Interaction>();
        var algorithm = posts is null && interactions is null
            ? new RecommendationAlgorithm()
            : AlgorithmFrom(resolvedPosts, resolvedInteractions);
        var user = User(
            resume: resume,
            preferredLocation: preferredLocation,
            preferredEmploymentType: preferredEmploymentType);
        var job = Job(
            description: jobDescription,
            location: jobLocation,
            employmentType: jobEmploymentType,
            promotionLevel: promotionLevel);

        return algorithm.CalculateScoreBreakdown(
            user,
            job,
            userSkills ?? Array.Empty<Skill>(),
            jobSkills ?? Array.Empty<Skill>());
    }

    private static RecommendationAlgorithm AlgorithmFrom(IReadOnlyList<Post> posts, IReadOnlyList<Interaction> interactions)
    {
        return new RecommendationAlgorithm(new FakePostRepository(posts), new FakeInteractionRepository(interactions));
    }

    private static Scenario ScenarioWithOnlyComponentAtOneHundred(PostParameterType component)
    {
        var user = User(resume: "developer", preferredLocation: "Remote", preferredEmploymentType: "Contract");
        var job = Job(description: "manager", location: "Office", employmentType: "Full-time", promotionLevel: 0);
        var userSkills = Array.Empty<Skill>();
        var jobSkills = Array.Empty<Skill>();

        if (component == PostParameterType.WeightedDistanceScoreWeight)
        {
            userSkills = new[] { Skill(1, "C#", 80) };
            jobSkills = new[] { Skill(1, "C#", 80) };
        }
        else if (component == PostParameterType.JobResumeSimilarityScoreWeight)
        {
            user.Resume = "shared";
            job.JobDescription = "shared";
        }
        else if (component == PostParameterType.PreferenceScoreWeight)
        {
            job.Location = user.PreferredLocation;
            job.EmploymentType = user.PreferredEmploymentType;
        }
        else if (component == PostParameterType.PromotionScoreWeight)
        {
            job.PromotionLevel = 100;
        }

        return new Scenario(user, job, userSkills, jobSkills);
    }

    private static double DefaultWeightedTotal(CompatibilityBreakdown breakdown)
    {
        return (breakdown.SkillScore + breakdown.KeywordScore + breakdown.PreferenceScore + breakdown.PromotionScore) / 4.0;
    }

    private static User User(
        string resume = "resume",
        string preferredLocation = "Cluj",
        string preferredEmploymentType = "Full-time")
    {
        return new User
        {
            UserId = 1,
            Name = "Test User",
            Location = "Cluj",
            PreferredLocation = preferredLocation,
            Email = "user@example.test",
            Phone = "0700000000",
            YearsOfExperience = 3,
            Education = "Computer Science",
            Resume = resume,
            PreferredEmploymentType = preferredEmploymentType
        };
    }

    private static Job Job(
        string description = "job",
        string location = "Cluj",
        string employmentType = "Full-time",
        int promotionLevel = 0)
    {
        return new Job
        {
            JobId = 100,
            JobTitle = "Test Job",
            JobDescription = description,
            Location = location,
            EmploymentType = employmentType,
            CompanyId = 1,
            PromotionLevel = promotionLevel
        };
    }

    private static Skill Skill(int skillId, string name, int score)
    {
        return new Skill
        {
            UserId = 1,
            SkillId = skillId,
            SkillName = name,
            Score = score
        };
    }

    private static JobSkill JobSkill(int skillId, string name, int score)
    {
        return new JobSkill
        {
            JobId = 100,
            SkillId = skillId,
            SkillName = name,
            Score = score
        };
    }

    private static IReadOnlyList<Post> WeightPosts(double skill, double resume, double preference, double promotion)
    {
        return new[]
        {
            Post(1, PostParameterType.WeightedDistanceScoreWeight, skill.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Post(2, PostParameterType.JobResumeSimilarityScoreWeight, resume.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Post(3, PostParameterType.PreferenceScoreWeight, preference.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            Post(4, PostParameterType.PromotionScoreWeight, promotion.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
    }

    private static Post Post(int postId, PostParameterType parameterType, string value)
    {
        return new Post
        {
            PostId = postId,
            DeveloperId = 1,
            ParameterType = parameterType,
            Value = value
        };
    }

    private static Interaction Like(int postId, int developerId = 1)
    {
        return Interaction(postId, developerId, InteractionType.Like);
    }

    private static Interaction Dislike(int postId, int developerId = 1)
    {
        return Interaction(postId, developerId, InteractionType.Dislike);
    }

    private static Interaction Interaction(int postId, int developerId, InteractionType type)
    {
        return new Interaction
        {
            InteractionId = (postId * 1000) + developerId,
            DeveloperId = developerId,
            PostId = postId,
            Type = type
        };
    }

    private static IReadOnlyList<Interaction> LikesForPost(int postId, int count)
    {
        return Enumerable.Range(1, count).Select(developerId => Like(postId, developerId)).ToList();
    }

    private sealed class Scenario
    {
        public Scenario(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills)
        {
            User = user;
            Job = job;
            UserSkills = userSkills;
            JobSkills = jobSkills;
        }

        public User User { get; }
        public Job Job { get; }
        public IReadOnlyList<Skill> UserSkills { get; }
        public IReadOnlyList<Skill> JobSkills { get; }
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
