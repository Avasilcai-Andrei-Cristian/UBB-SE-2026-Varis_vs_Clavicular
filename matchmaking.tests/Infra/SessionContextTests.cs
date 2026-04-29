namespace matchmaking.Tests;

public class SessionContextTests
{
    [Fact]
    public void LoginAsCompany_clears_user_and_developer_ids_and_sets_company_mode()
    {
        var ctx = new SessionContext();
        ctx.LoginAsUser(1);
        ctx.LoginAsDeveloper(2);

        ctx.LoginAsCompany(5);

        ctx.CurrentUserId.Should().BeNull();
        ctx.CurrentDeveloperId.Should().BeNull();
        ctx.CurrentCompanyId.Should().Be(5);
        ctx.CurrentMode.Should().Be(AppMode.CompanyMode);
    }

    [Fact]
    public void Default_session_has_UserMode_and_all_ids_null()
    {
        var ctx = new SessionContext();

        ctx.CurrentMode.Should().Be(AppMode.UserMode);
        ctx.CurrentUserId.Should().BeNull();
        ctx.CurrentCompanyId.Should().BeNull();
        ctx.CurrentDeveloperId.Should().BeNull();
    }
}
