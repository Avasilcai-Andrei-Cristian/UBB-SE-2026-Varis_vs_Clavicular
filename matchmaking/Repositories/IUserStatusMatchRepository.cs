using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IUserStatusMatchRepository
{
    IReadOnlyList<Match> GetByUserId(int userId);
    IReadOnlyList<Match> GetRejectedByUserId(int userId);
}
