using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public Company? GetById(int companyId) => companyRepository.GetById(companyId);
    public IReadOnlyList<Company> GetAll() => companyRepository.GetAll();
    public void Add(Company company) => companyRepository.Add(company);
    public void Update(Company company) => companyRepository.Update(company);
    public void Remove(int companyId) => companyRepository.Remove(companyId);
}
