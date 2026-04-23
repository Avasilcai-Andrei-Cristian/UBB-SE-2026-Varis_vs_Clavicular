using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Models;

public sealed class UserStatusJobDetailPayload
{
    public required ApplicationCardModel Card { get; init; }
    public required IReadOnlyList<JobSkill> JobSkills { get; init; }
}
