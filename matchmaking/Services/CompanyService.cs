using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public Company? GetById(int companyId) => _companyRepository.GetById(companyId);
    public IReadOnlyList<Company> GetAll() => _companyRepository.GetAll();
    public void Add(Company company) => _companyRepository.Add(company);
    public void Update(Company company) => _companyRepository.Update(company);
    public void Remove(int companyId) => _companyRepository.Remove(companyId);
}
