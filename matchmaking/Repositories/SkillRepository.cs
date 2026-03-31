using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class SkillRepository
{
    private readonly List<Skill> _skills =
    [
        new() { UserId = 1, SkillId = 1, SkillName = "C#", Score = 75 },
        new() { UserId = 1, SkillId = 2, SkillName = "React", Score = 82 },
        new() { UserId = 2, SkillId = 1, SkillName = "C#", Score = 88 },
        new() { UserId = 2, SkillId = 3, SkillName = "SQL", Score = 84 },
        new() { UserId = 2, SkillId = 2, SkillName = "React", Score = 62 },
        new() { UserId = 2, SkillId = 6, SkillName = "Docker", Score = 71 },
        new() { UserId = 2, SkillId = 8, SkillName = "Python", Score = 58 },
        new() { UserId = 2, SkillId = 14, SkillName = "Architecture", Score = 45 },
        new() { UserId = 2, SkillId = 15, SkillName = "Leadership", Score = 40 },
        new() { UserId = 3, SkillId = 4, SkillName = "Testing", Score = 70 },
        new() { UserId = 3, SkillId = 5, SkillName = "Selenium", Score = 65 },
        new() { UserId = 4, SkillId = 6, SkillName = "Docker", Score = 90 },
        new() { UserId = 4, SkillId = 7, SkillName = "Kubernetes", Score = 83 },
        new() { UserId = 5, SkillId = 8, SkillName = "Python", Score = 79 },
        new() { UserId = 5, SkillId = 9, SkillName = "Pandas", Score = 76 },
        new() { UserId = 6, SkillId = 10, SkillName = "Machine Learning", Score = 89 },
        new() { UserId = 6, SkillId = 11, SkillName = "NLP", Score = 85 },
        new() { UserId = 7, SkillId = 12, SkillName = "Figma", Score = 87 },
        new() { UserId = 7, SkillId = 13, SkillName = "UI Design", Score = 80 },
        new() { UserId = 8, SkillId = 14, SkillName = "Architecture", Score = 91 },
        new() { UserId = 8, SkillId = 15, SkillName = "Leadership", Score = 88 },
        new() { UserId = 9, SkillId = 2, SkillName = "React", Score = 77 },
        new() { UserId = 9, SkillId = 1, SkillName = "C#", Score = 74 },
        new() { UserId = 10, SkillId = 16, SkillName = "Cloud", Score = 92 },
        new() { UserId = 10, SkillId = 6, SkillName = "Docker", Score = 86 }
    ];

    public Skill? GetById(int userId, int skillId) =>
        _skills.FirstOrDefault(s => s.UserId == userId && s.SkillId == skillId);

    public IReadOnlyList<Skill> GetAll() => _skills.ToList();

    public IReadOnlyList<Skill> GetByUserId(int userId) =>
        _skills.Where(s => s.UserId == userId).ToList();

    public void Add(Skill skill)
    {
        if (_skills.Any(s => s.UserId == skill.UserId && s.SkillId == skill.SkillId))
        {
            throw new InvalidOperationException($"Skill ({skill.UserId}, {skill.SkillId}) already exists.");
        }

        _skills.Add(skill);
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
        _skills.Remove(existing);
    }
}
