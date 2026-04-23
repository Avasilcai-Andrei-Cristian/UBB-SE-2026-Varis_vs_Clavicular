using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly List<Skill> skills =
    [
        new () { UserId = 1, SkillId = 1, SkillName = "C#", Score = 75 },
        new () { UserId = 1, SkillId = 2, SkillName = "React", Score = 82 },
        new () { UserId = 2, SkillId = 1, SkillName = "C#", Score = 88 },
        new () { UserId = 2, SkillId = 3, SkillName = "SQL", Score = 84 },
        new () { UserId = 2, SkillId = 2, SkillName = "React", Score = 62 },
        new () { UserId = 2, SkillId = 6, SkillName = "Docker", Score = 71 },
        new () { UserId = 2, SkillId = 8, SkillName = "Python", Score = 58 },
        new () { UserId = 2, SkillId = 14, SkillName = "Architecture", Score = 45 },
        new () { UserId = 2, SkillId = 15, SkillName = "Leadership", Score = 40 },
        new () { UserId = 3, SkillId = 4, SkillName = "Testing", Score = 70 },
        new () { UserId = 3, SkillId = 5, SkillName = "Selenium", Score = 65 },
        new () { UserId = 4, SkillId = 6, SkillName = "Docker", Score = 90 },
        new () { UserId = 4, SkillId = 7, SkillName = "Kubernetes", Score = 83 },
        new () { UserId = 5, SkillId = 8, SkillName = "Python", Score = 79 },
        new () { UserId = 5, SkillId = 9, SkillName = "Pandas", Score = 76 },
        new () { UserId = 6, SkillId = 10, SkillName = "Machine Learning", Score = 89 },
        new () { UserId = 6, SkillId = 11, SkillName = "NLP", Score = 85 },
        new () { UserId = 7, SkillId = 12, SkillName = "Figma", Score = 87 },
        new () { UserId = 7, SkillId = 13, SkillName = "UI Design", Score = 80 },
        new () { UserId = 8, SkillId = 14, SkillName = "Architecture", Score = 91 },
        new () { UserId = 8, SkillId = 15, SkillName = "Leadership", Score = 88 },
        new () { UserId = 9, SkillId = 2, SkillName = "React", Score = 77 },
        new () { UserId = 9, SkillId = 1, SkillName = "C#", Score = 74 },
        new () { UserId = 10, SkillId = 16, SkillName = "Cloud", Score = 92 },
        new () { UserId = 10, SkillId = 6, SkillName = "Docker", Score = 86 },
        new () { UserId = 11, SkillId = 17, SkillName = "Flutter", Score = 80 },
        new () { UserId = 11, SkillId = 18, SkillName = "Kotlin", Score = 74 },
        new () { UserId = 12, SkillId = 19, SkillName = "Penetration Testing", Score = 88 },
        new () { UserId = 12, SkillId = 20, SkillName = "SIEM", Score = 81 },
        new () { UserId = 12, SkillId = 6, SkillName = "Docker", Score = 65 },
        new () { UserId = 13, SkillId = 21, SkillName = "Java", Score = 68 },
        new () { UserId = 13, SkillId = 22, SkillName = "Spring Boot", Score = 60 },
        new () { UserId = 14, SkillId = 15, SkillName = "Leadership", Score = 93 },
        new () { UserId = 14, SkillId = 14, SkillName = "Architecture", Score = 87 },
        new () { UserId = 14, SkillId = 23, SkillName = "Agile", Score = 85 },
        new () { UserId = 15, SkillId = 8, SkillName = "Python", Score = 82 },
        new () { UserId = 15, SkillId = 24, SkillName = "Spark", Score = 78 },
        new () { UserId = 15, SkillId = 3, SkillName = "SQL", Score = 80 },
        new () { UserId = 16, SkillId = 25, SkillName = "Go", Score = 75 },
        new () { UserId = 16, SkillId = 26, SkillName = "PostgreSQL", Score = 70 },
        new () { UserId = 17, SkillId = 27, SkillName = "Computer Vision", Score = 86 },
        new () { UserId = 17, SkillId = 28, SkillName = "PyTorch", Score = 82 },
        new () { UserId = 17, SkillId = 8, SkillName = "Python", Score = 84 },
        new () { UserId = 18, SkillId = 29, SkillName = "Angular", Score = 76 },
        new () { UserId = 18, SkillId = 1, SkillName = "C#", Score = 73 },
        new () { UserId = 18, SkillId = 3, SkillName = "SQL", Score = 68 },
        new () { UserId = 19, SkillId = 30, SkillName = "Vue.js", Score = 79 },
        new () { UserId = 19, SkillId = 31, SkillName = "TypeScript", Score = 75 },
        new () { UserId = 20, SkillId = 16, SkillName = "Cloud", Score = 90 },
        new () { UserId = 20, SkillId = 32, SkillName = "AWS", Score = 88 },
        new () { UserId = 20, SkillId = 6, SkillName = "Docker", Score = 82 },
        new () { UserId = 20, SkillId = 7, SkillName = "Kubernetes", Score = 80 }
    ];

    public Skill? GetById(int userId, int skillId) =>
        skills.FirstOrDefault(s => s.UserId == userId && s.SkillId == skillId);

    public IReadOnlyList<Skill> GetAll() => skills.ToList();

    public IReadOnlyList<Skill> GetByUserId(int userId) =>
        skills.Where(s => s.UserId == userId).ToList();

    public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() =>
        skills
            .GroupBy(s => s.SkillId)
            .Select(g => (SkillId: g.Key, Name: g.First().SkillName))
            .OrderBy(t => t.Name)
            .ToList();

    public void Add(Skill skill)
    {
        if (skills.Any(s => s.UserId == skill.UserId && s.SkillId == skill.SkillId))
        {
            throw new InvalidOperationException($"Skill ({skill.UserId}, {skill.SkillId}) already exists.");
        }

        skills.Add(skill);
    }

    public void Update(Skill skill)
    {
        var existing = GetById(skill.UserId, skill.SkillId)
            ?? throw new KeyNotFoundException($"Skill ({skill.UserId}, {skill.SkillId}) was not found.");
        existing.SkillName = skill.SkillName;
        existing.Score = skill.Score;
    }

    public void Remove(int userId, int skillId)
    {
        var existing = GetById(userId, skillId)
            ?? throw new KeyNotFoundException($"Skill ({userId}, {skillId}) was not found.");
        skills.Remove(existing);
    }
}
