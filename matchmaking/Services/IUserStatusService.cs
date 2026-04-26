using System.Collections.Generic;
using matchmaking.Models;

namespace matchmaking.Services
{
    public interface IUserStatusService
    {
        IReadOnlyList<ApplicationCardModel> GetApplicationsForUser(int userId);
    }
}