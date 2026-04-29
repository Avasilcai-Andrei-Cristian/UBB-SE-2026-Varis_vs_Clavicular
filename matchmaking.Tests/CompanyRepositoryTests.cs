using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class CompanyRepositoryTests
{
    [Fact]
    public void GetById_returns_company_when_company_id_exists()
    {
        const int existingCompanyId = 3;
        var existingCompany = new Company
        {
            CompanyId = existingCompanyId,
            CompanyName = "DataForge",
            LogoText = "DF",
            Email = "careers@dataforge.com",
            Phone = "0311000003"
        };
        var repository = new CompanyRepository(new[] { existingCompany });
        var result = repository.GetById(existingCompanyId);
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("DataForge");
        result.Email.Should().Be("careers@dataforge.com");
    }

    [Fact]
    public void Update_replaces_name_email_and_phone_of_existing_company()
    {
        const int companyId = 6;
        var originalCompany = new Company
        {
            CompanyId = companyId,
            CompanyName = "GreenCode",
            LogoText = "GC",
            Email = "work@greencode.com",
            Phone = "0311000006"
        };
        var repository = new CompanyRepository(new[] { originalCompany });
        var updatedCompany = new Company
        {
            CompanyId = companyId,
            CompanyName = "GreenCode Rebranded",
            LogoText = "GR",
            Email = "hr@greencode-rebranded.com",
            Phone = "0311009999"
        };
        repository.Update(updatedCompany);
        var retrieved = repository.GetById(companyId);
        retrieved!.CompanyName.Should().Be("GreenCode Rebranded");
        retrieved.LogoText.Should().Be("GR");
        retrieved.Email.Should().Be("hr@greencode-rebranded.com");
        retrieved.Phone.Should().Be("0311009999");
    }
}