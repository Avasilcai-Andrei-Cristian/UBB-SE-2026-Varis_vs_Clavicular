using System.Linq;

namespace matchmaking.Tests;

[Collection("SqlIntegration")]
public sealed class SqlPostRepositoryIntegrationTests
{
    private readonly SqlIntegrationTestDatabase database;
    public SqlPostRepositoryIntegrationTests(SqlIntegrationTestDatabase database)
    {
        this.database = database;
        this.database.ResetAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Add_inserts_post_and_it_can_be_retrieved_by_developer_id()
    {
        var developerRepository = new SqlDeveloperRepository(database.ConnectionString);
        var postRepository = new SqlPostRepository(database.ConnectionString);
        developerRepository.Add(new Developer { Name = "poster_dev", Password = "pass" });
        var savedDeveloper = developerRepository.GetAll().Single();
        postRepository.Add(new Post
        {
            DeveloperId = savedDeveloper.DeveloperId,
            ParameterType = PostParameterType.RelevantKeyword,
            Value = "Kubernetes"
        });
        var posts = postRepository.GetByDeveloperId(savedDeveloper.DeveloperId);
        posts.Should().ContainSingle();
        posts[0].Value.Should().Be("Kubernetes");
    }

    [Fact]
    public void GetByDeveloperId_returns_only_posts_belonging_to_that_developer()
    {
        var developerRepository = new SqlDeveloperRepository(database.ConnectionString);
        var postRepository = new SqlPostRepository(database.ConnectionString);
        developerRepository.Add(new Developer { Name = "dev_filter_one", Password = "pass" });
        developerRepository.Add(new Developer { Name = "dev_filter_two", Password = "pass" });
        var allDevelopers = developerRepository.GetAll();
        var firstDeveloper = allDevelopers[0];
        var secondDeveloper = allDevelopers[1];
        postRepository.Add(new Post
        {
            DeveloperId = firstDeveloper.DeveloperId,
            ParameterType = PostParameterType.MitigationFactor,
            Value = "0.3"
        });
        postRepository.Add(new Post
        {
            DeveloperId = firstDeveloper.DeveloperId,
            ParameterType = PostParameterType.PromotionScoreWeight,
            Value = "1.5"
        });
        postRepository.Add(new Post
        {
            DeveloperId = secondDeveloper.DeveloperId,
            ParameterType = PostParameterType.RelevantKeyword,
            Value = "React"
        });
        var firstDeveloperPosts = postRepository.GetByDeveloperId(firstDeveloper.DeveloperId);
        firstDeveloperPosts.Should().HaveCount(2);
        firstDeveloperPosts.Should().AllSatisfy(post => post.DeveloperId.Should().Be(firstDeveloper.DeveloperId));
    }

    [Fact]
    public void Add_round_trips_post_parameter_type_through_storage()
    {
        var developerRepository = new SqlDeveloperRepository(database.ConnectionString);
        var postRepository = new SqlPostRepository(database.ConnectionString);
        developerRepository.Add(new Developer { Name = "dev_param_type", Password = "pass" });
        var savedDeveloper = developerRepository.GetAll().Single();
        postRepository.Add(new Post
        {
            DeveloperId = savedDeveloper.DeveloperId,
            ParameterType = PostParameterType.JobResumeSimilarityScoreWeight,
            Value = "2.0"
        });
        var savedPost = postRepository.GetByDeveloperId(savedDeveloper.DeveloperId).Single();
        savedPost.ParameterType.Should().Be(PostParameterType.JobResumeSimilarityScoreWeight);
    }
}