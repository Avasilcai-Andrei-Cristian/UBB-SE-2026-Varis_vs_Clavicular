using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Services;

public interface ICompanyService
{
    Company? GetById(int companyId);
    IReadOnlyList<Company> GetAll();
    void Add(Company company);
    void Update(Company company);
    void Remove(int companyId);
}
