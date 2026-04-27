using System.Collections.Generic;
using matchmaking.Models;

namespace matchmaking.Services
{
    public interface ISkillGapService
    {
        IReadOnlyList<MissingSkillModel> GetMissingSkills(int userId);
        SkillGapSummaryModel GetSummary(int userId);
        IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId);
    }
}