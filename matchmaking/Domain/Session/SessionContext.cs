using matchmaking.Domain.Enums;

namespace matchmaking.Domain.Session;

public class SessionContext
{
    public int? CurrentUserId { get; private set; }
    public int? CurrentCompanyId { get; private set; }
    public int? CurrentDeveloperId { get; private set; }
    public AppMode CurrentMode { get; private set; }

    public void LoginAsUser(int userId)
    {
        CurrentUserId = userId;
        CurrentCompanyId = null;
        CurrentDeveloperId = null;
        CurrentMode = AppMode.UserMode;
    }

    public void LoginAsCompany(int companyId)
    {
        CurrentUserId = null;
        CurrentCompanyId = companyId;
        CurrentDeveloperId = null;
        CurrentMode = AppMode.CompanyMode;
    }

    public void LoginAsDeveloper(int developerId)
    {
        CurrentUserId = null;
        CurrentCompanyId = null;
        CurrentDeveloperId = developerId;
        CurrentMode = AppMode.DeveloperMode;
    }

    public void Logout()
    {
        CurrentUserId = null;
        CurrentCompanyId = null;
        CurrentDeveloperId = null;
        CurrentMode = AppMode.UserMode;
    }
}
