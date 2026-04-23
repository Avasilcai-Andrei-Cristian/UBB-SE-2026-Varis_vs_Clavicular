using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface ICompanyRepository
{
    Company? GetById(int companyId);
    IReadOnlyList<Company> GetAll();
    void Add(Company company);
    void Update(Company company);
    void Remove(int companyId);
}
