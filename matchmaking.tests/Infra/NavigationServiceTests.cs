namespace matchmaking.Tests;

public class NavigationServiceTests
{
    private readonly NavigationService service = new();

    [Fact]
    public void RequestUserProfile_fires_UserProfileRequested_with_correct_id()
    {
        int? captured = null;
        service.UserProfileRequested += id => captured = id;

        service.RequestUserProfile(42);

        captured.Should().Be(42);
    }

    [Fact]
    public void RequestCompanyProfile_fires_CompanyProfileRequested_with_correct_id()
    {
        int? captured = null;
        service.CompanyProfileRequested += id => captured = id;

        service.RequestCompanyProfile(7);

        captured.Should().Be(7);
    }

    [Fact]
    public void RequestJobPost_fires_JobPostRequested_with_correct_id()
    {
        int? captured = null;
        service.JobPostRequested += id => captured = id;

        service.RequestJobPost(99);

        captured.Should().Be(99);
    }
}
