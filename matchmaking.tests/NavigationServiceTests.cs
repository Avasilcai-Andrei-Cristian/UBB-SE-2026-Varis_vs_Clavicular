namespace matchmaking.Tests;

public sealed class NavigationServiceTests
{
    [Fact]
    public void RequestUserProfile_WhenSubscriberExists_RaisesEventWithProvidedUserId()
    {
        var service = new NavigationService();
        var raisedUserId = -1;
        var raisedCount = 0;
        service.UserProfileRequested += userId =>
        {
            raisedUserId = userId;
            raisedCount++;
        };

        service.RequestUserProfile(42);

        raisedCount.Should().Be(1);
        raisedUserId.Should().Be(42);
    }

    [Fact]
    public void RequestCompanyProfile_WhenSubscriberExists_RaisesEventWithProvidedCompanyId()
    {
        var service = new NavigationService();
        var raisedCompanyId = -1;
        var raisedCount = 0;
        service.CompanyProfileRequested += companyId =>
        {
            raisedCompanyId = companyId;
            raisedCount++;
        };

        service.RequestCompanyProfile(7);

        raisedCount.Should().Be(1);
        raisedCompanyId.Should().Be(7);
    }

    [Fact]
    public void RequestJobPost_WhenSubscriberExists_RaisesEventWithProvidedJobId()
    {
        var service = new NavigationService();
        var raisedJobId = -1;
        var raisedCount = 0;
        service.JobPostRequested += jobId =>
        {
            raisedJobId = jobId;
            raisedCount++;
        };

        service.RequestJobPost(100);

        raisedCount.Should().Be(1);
        raisedJobId.Should().Be(100);
    }

}
