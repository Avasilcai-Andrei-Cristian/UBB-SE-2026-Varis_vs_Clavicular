using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class UserRepository : IUserRepository
{
    private readonly List<User> users =
    [
        new () { UserId = 1, Name = "Alice Pop", Location = "Cluj-Napoca", PreferredLocation = "Cluj-Napoca", Email = "alice.pop@mail.com", Phone = "0700000001", YearsOfExperience = 2, Education = "BSc Computer Science", Resume = "Frontend developer with React projects", PreferredEmploymentType = "Full-time" },
        new () { UserId = 2, Name = "Bogdan Ionescu", Location = "Bucharest", PreferredLocation = "Bucharest", Email = "bogdan.ionescu@mail.com", Phone = "0700000002", YearsOfExperience = 4, Education = "MSc Software Engineering", Resume = "Backend .NET and SQL developer", PreferredEmploymentType = "Full-time" },
        new () { UserId = 3, Name = "Carmen Radu", Location = "Iasi", PreferredLocation = "Iasi", Email = "carmen.radu@mail.com", Phone = "0700000003", YearsOfExperience = 1, Education = "BSc Informatics", Resume = "Junior QA and automation", PreferredEmploymentType = "Internship" },
        new () { UserId = 4, Name = "Dan Tudor", Location = "Timisoara", PreferredLocation = "Timisoara", Email = "dan.tudor@mail.com", Phone = "0700000004", YearsOfExperience = 6, Education = "BSc Computer Engineering", Resume = "DevOps and cloud pipelines", PreferredEmploymentType = "Remote" },
        new () { UserId = 5, Name = "Elena Matei", Location = "Brasov", PreferredLocation = "Brasov", Email = "elena.matei@mail.com", Phone = "0700000005", YearsOfExperience = 3, Education = "BSc Mathematics and CS", Resume = "Data analyst and Python scripting", PreferredEmploymentType = "Hybrid" },
        new () { UserId = 6, Name = "Florin Pavel", Location = "Oradea", PreferredLocation = "Oradea", Email = "florin.pavel@mail.com", Phone = "0700000006", YearsOfExperience = 5, Education = "MSc AI", Resume = "ML engineer with NLP focus", PreferredEmploymentType = "Full-time" },
        new () { UserId = 7, Name = "Gabriela Stan", Location = "Sibiu", PreferredLocation = "Sibiu", Email = "gabriela.stan@mail.com", Phone = "0700000007", YearsOfExperience = 2, Education = "BSc Computer Science", Resume = "UI/UX and mobile interfaces", PreferredEmploymentType = "Part-time" },
        new () { UserId = 8, Name = "Horia Vasile", Location = "Constanta", PreferredLocation = "Constanta", Email = "horia.vasile@mail.com", Phone = "0700000008", YearsOfExperience = 7, Education = "BSc Information Systems", Resume = "Tech lead for enterprise systems", PreferredEmploymentType = "Hybrid" },
        new () { UserId = 9, Name = "Ioana Dobre", Location = "Craiova", PreferredLocation = "Craiova", Email = "ioana.dobre@mail.com", Phone = "0700000009", YearsOfExperience = 3, Education = "BSc Computer Science", Resume = "Full-stack web developer", PreferredEmploymentType = "Remote" },
        new () { UserId = 10, Name = "Julian Muresan", Location = "Targu Mures", PreferredLocation = "Targu Mures", Email = "julian.muresan@mail.com", Phone = "0700000010", YearsOfExperience = 8, Education = "MSc Distributed Systems", Resume = "Architecture and mentoring", PreferredEmploymentType = "Full-time" },
        new () { UserId = 11, Name = "Katerina Lupu", Location = "Cluj-Napoca", PreferredLocation = "Cluj-Napoca", Email = "katerina.lupu@mail.com", Phone = "0700000011", YearsOfExperience = 3, Education = "BSc Computer Science", Resume = "Mobile developer with Flutter and Kotlin experience", PreferredEmploymentType = "Full-time" },
        new () { UserId = 12, Name = "Lucian Barbu", Location = "Bucharest", PreferredLocation = "Bucharest", Email = "lucian.barbu@mail.com", Phone = "0700000012", YearsOfExperience = 5, Education = "MSc Cybersecurity", Resume = "Security engineer with pen-testing and SIEM tools", PreferredEmploymentType = "Remote" },
        new () { UserId = 13, Name = "Maria Enescu", Location = "Timisoara", PreferredLocation = "Timisoara", Email = "maria.enescu@mail.com", Phone = "0700000013", YearsOfExperience = 1, Education = "BSc Computer Science", Resume = "Recent grad with Java and Spring Boot projects", PreferredEmploymentType = "Internship" },
        new () { UserId = 14, Name = "Nicolae Grigorescu", Location = "Brasov", PreferredLocation = "Brasov", Email = "nicolae.grigorescu@mail.com", Phone = "0700000014", YearsOfExperience = 10, Education = "MSc Software Engineering", Resume = "Engineering manager, scaled teams from 5 to 30", PreferredEmploymentType = "Full-time" },
        new () { UserId = 15, Name = "Oana Cristea", Location = "Iasi", PreferredLocation = "Iasi", Email = "oana.cristea@mail.com", Phone = "0700000015", YearsOfExperience = 4, Education = "BSc Mathematics and CS", Resume = "Data engineer with Spark, Airflow, and SQL pipelines", PreferredEmploymentType = "Hybrid" },
        new () { UserId = 16, Name = "Pavel Dragomir", Location = "Oradea", PreferredLocation = "Oradea", Email = "pavel.dragomir@mail.com", Phone = "0700000016", YearsOfExperience = 2, Education = "BSc Informatics", Resume = "Backend developer with Go and PostgreSQL experience", PreferredEmploymentType = "Remote" },
        new () { UserId = 17, Name = "Raluca Stoica", Location = "Sibiu", PreferredLocation = "Sibiu", Email = "raluca.stoica@mail.com", Phone = "0700000017", YearsOfExperience = 6, Education = "MSc AI", Resume = "Computer vision engineer with PyTorch and OpenCV", PreferredEmploymentType = "Full-time" },
        new () { UserId = 18, Name = "Stefan Marinescu", Location = "Constanta", PreferredLocation = "Constanta", Email = "stefan.marinescu@mail.com", Phone = "0700000018", YearsOfExperience = 3, Education = "BSc Computer Science", Resume = "Full-stack developer with Angular and .NET Core", PreferredEmploymentType = "Hybrid" },
        new () { UserId = 19, Name = "Teodora Voinea", Location = "Craiova", PreferredLocation = "Craiova", Email = "teodora.voinea@mail.com", Phone = "0700000019", YearsOfExperience = 2, Education = "BSc Computer Science", Resume = "Frontend developer with Vue.js and TypeScript", PreferredEmploymentType = "Full-time" },
        new () { UserId = 20, Name = "Vlad Petrescu", Location = "Cluj-Napoca", PreferredLocation = "Cluj-Napoca", Email = "vlad.petrescu@mail.com", Phone = "0700000020", YearsOfExperience = 7, Education = "MSc Cloud Computing", Resume = "Cloud architect with AWS and Azure certifications", PreferredEmploymentType = "Remote" }
    ];

    public User? GetById(int userId) => users.FirstOrDefault(u => u.UserId == userId);

    public IReadOnlyList<User> GetAll() => users.ToList();

    public void Add(User user)
    {
        if (users.Any(u => u.UserId == user.UserId))
        {
            throw new InvalidOperationException($"User with id {user.UserId} already exists.");
        }

        users.Add(user);
    }

    public void Update(User user)
    {
        var existing = GetById(user.UserId) ?? throw new KeyNotFoundException($"User with id {user.UserId} was not found.");
        existing.Name = user.Name;
        existing.Location = user.Location;
        existing.PreferredLocation = user.PreferredLocation;
        existing.Email = user.Email;
        existing.Phone = user.Phone;
        existing.YearsOfExperience = user.YearsOfExperience;
        existing.Education = user.Education;
        existing.Resume = user.Resume;
        existing.PreferredEmploymentType = user.PreferredEmploymentType;
    }

    public void Remove(int userId)
    {
        var existing = GetById(userId) ?? throw new KeyNotFoundException($"User with id {userId} was not found.");
        users.Remove(existing);
    }
}
