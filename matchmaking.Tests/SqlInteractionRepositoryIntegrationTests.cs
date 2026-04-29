using System.Linq;

namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlInteractionRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;

    public SqlInteractionRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Add_records_like_interaction_for_developer_and_post()
    {
        var developerRepository = new SqlDeveloperRepository(database.ConnectionString);
        var postRepository = new SqlPostRepository(database.ConnectionString);
        var interactionRepository = new SqlInteractionRepository(database.ConnectionString);
        developerRepository.Add(new Developer { Name = "dev_like", Password = "pass" });
        var savedDeveloper = developerRepository.GetAll().Single();
        postRepository.Add(new Post
        {
            DeveloperId = savedDeveloper.DeveloperId,
            ParameterType = PostParameterType.RelevantKeyword,
            Value = "Python"
        });
        var savedPost = postRepository.GetByDeveloperId(savedDeveloper.DeveloperId).Single();
        interactionRepository.Add(new Interaction
        {
            DeveloperId = savedDeveloper.DeveloperId,
            PostId = savedPost.PostId,
            Type = InteractionType.Like
        });
        var all = interactionRepository.GetAll();
        all.Should().ContainSingle();
        all[0].Type.Should().Be(InteractionType.Like);
    }

    [Fact]
    public void GetAll_returns_all_recorded_interactions()
    {
        var developerRepository = new SqlDeveloperRepository(database.ConnectionString);
        var postRepository = new SqlPostRepository(database.ConnectionString);
        var interactionRepository = new SqlInteractionRepository(database.ConnectionString);
        developerRepository.Add(new Developer { Name = "dev_all", Password = "pass" });
        var savedDeveloper = developerRepository.GetAll().Single();
        postRepository.Add(new Post
        {
            DeveloperId = savedDeveloper.DeveloperId,
            ParameterType = PostParameterType.MitigationFactor,
            Value = "0.5"
        });
        postRepository.Add(new Post
        {
            DeveloperId = savedDeveloper.DeveloperId,
            ParameterType = PostParameterType.PromotionScoreWeight,
            Value = "1.2"
        });
        var posts = postRepository.GetByDeveloperId(savedDeveloper.DeveloperId);
        interactionRepository.Add(new Interaction
        {
            DeveloperId = savedDeveloper.DeveloperId,
            PostId = posts[0].PostId,
            Type = InteractionType.Like
        });
        interactionRepository.Add(new Interaction
        {
            DeveloperId = savedDeveloper.DeveloperId,
            PostId = posts[1].PostId,
            Type = InteractionType.Dislike
        });
        var all = interactionRepository.GetAll();
        all.Should().HaveCount(2);
    }

    [Fact]
    public void GetByDeveloperIdAndPostId_returns_single_interaction_for_developer_post_pair()
    {
        var developerRepository = new SqlDeveloperRepository(database.ConnectionString);
        var postRepository = new SqlPostRepository(database.ConnectionString);
        var interactionRepository = new SqlInteractionRepository(database.ConnectionString);
        developerRepository.Add(new Developer { Name = "dev_dedupe", Password = "pass" });
        var savedDeveloper = developerRepository.GetAll().Single();
        postRepository.Add(new Post
        {
            DeveloperId = savedDeveloper.DeveloperId,
            ParameterType = PostParameterType.RelevantKeyword,
            Value = "Docker"
        });
        var savedPost = postRepository.GetByDeveloperId(savedDeveloper.DeveloperId).Single();
        interactionRepository.Add(new Interaction
        {
            DeveloperId = savedDeveloper.DeveloperId,
            PostId = savedPost.PostId,
            Type = InteractionType.Dislike
        });
        var result = interactionRepository.GetByDeveloperIdAndPostId(savedDeveloper.DeveloperId, savedPost.PostId);
        result.Should().NotBeNull();
        result!.DeveloperId.Should().Be(savedDeveloper.DeveloperId);
        result.PostId.Should().Be(savedPost.PostId);
        result.Type.Should().Be(InteractionType.Dislike);
    }
}