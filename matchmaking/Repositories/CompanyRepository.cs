using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly List<Company> companies;

    public CompanyRepository()
        : this(CreateDefaultCompanies())
    {
    }

    public CompanyRepository(IEnumerable<Company> initialCompanies)
    {
        companies = initialCompanies.ToList();
    }

    private static IEnumerable<Company> CreateDefaultCompanies()
    {
        return [
        new () { CompanyId = 1, CompanyName = "TechNova", LogoText = "TN", Email = "hr@technova.com", Phone = "0311000001" },
        new () { CompanyId = 2, CompanyName = "CloudWorks", LogoText = "CW", Email = "jobs@cloudworks.com", Phone = "0311000002" },
        new () { CompanyId = 3, CompanyName = "DataForge", LogoText = "DF", Email = "careers@dataforge.com", Phone = "0311000003" },
        new () { CompanyId = 4, CompanyName = "PixelSoft", LogoText = "PS", Email = "talent@pixelsoft.com", Phone = "0311000004" },
        new () { CompanyId = 5, CompanyName = "SecureOps", LogoText = "SO", Email = "team@secureops.com", Phone = "0311000005" },
        new () { CompanyId = 6, CompanyName = "GreenCode", LogoText = "GC", Email = "work@greencode.com", Phone = "0311000006" },
        new () { CompanyId = 7, CompanyName = "RocketApps", LogoText = "RA", Email = "apply@rocketapps.com", Phone = "0311000007" },
        new () { CompanyId = 8, CompanyName = "BrightSystems", LogoText = "BS", Email = "people@brightsystems.com", Phone = "0311000008" },
        new () { CompanyId = 9, CompanyName = "AI Valley", LogoText = "AV", Email = "hr@aivalley.com", Phone = "0311000009" },
        new () { CompanyId = 10, CompanyName = "CodeBridge", LogoText = "CB", Email = "careers@codebridge.com", Phone = "0311000010" }
        ];
    }

    public Company? GetById(int companyId)
    {
        foreach (var company in companies)
        {
            if (company.CompanyId == companyId)
            {
                return company;
            }
        }

        return null;
    }

    public IReadOnlyList<Company> GetAll() => companies.ToList();

    public void Add(Company company)
    {
        if (HasCompanyId(company.CompanyId))
        {
            throw new InvalidOperationException($"Company with id {company.CompanyId} already exists.");
        }

        companies.Add(company);
    }

    public void Update(Company company)
    {
        var existing = GetById(company.CompanyId) ?? throw new KeyNotFoundException($"Company with id {company.CompanyId} was not found.");
        existing.CompanyName = company.CompanyName;
        existing.LogoText = company.LogoText;
        existing.Email = company.Email;
        existing.Phone = company.Phone;
    }

    public void Remove(int companyId)
    {
        var existing = GetById(companyId) ?? throw new KeyNotFoundException($"Company with id {companyId} was not found.");
        companies.Remove(existing);
    }

    private bool HasCompanyId(int companyId)
    {
        foreach (var company in companies)
        {
            if (company.CompanyId == companyId)
            {
                return true;
            }
        }

        return false;
    }
}
