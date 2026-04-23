namespace matchmaking.Tests;

public sealed class CompanyServiceTests
{
    [Fact]
    public void GetById_WhenCompanyExists_ReturnsCompany()
    {
        var existingCompany = TestDataFactory.CreateCompany(4);
        var repository = new FakeCompanyRepository(new[] { existingCompany });
        var service = new CompanyService(repository);

        service.GetById(existingCompany.CompanyId).Should().Be(existingCompany);
    }

    [Fact]
    public void GetAll_WhenCompaniesExist_ReturnsCompanies()
    {
        var existingCompany = TestDataFactory.CreateCompany(4);
        var repository = new FakeCompanyRepository(new[] { existingCompany });
        var service = new CompanyService(repository);

        service.GetAll().Should().ContainSingle().Which.Should().Be(existingCompany);
    }

    [Fact]
    public void Add_WhenCompanyAdded_DelegatesToRepository()
    {
        var repository = new FakeCompanyRepository(Array.Empty<Company>());
        var service = new CompanyService(repository);
        var newCompany = TestDataFactory.CreateCompany(5);

        service.Add(newCompany);

        repository.AddedCompanies.Should().ContainSingle().Which.Should().Be(newCompany);
    }

    [Fact]
    public void Update_WhenCompanyUpdated_DelegatesToRepository()
    {
        var existingCompany = TestDataFactory.CreateCompany(4);
        var repository = new FakeCompanyRepository(new[] { existingCompany });
        var service = new CompanyService(repository);

        service.Update(existingCompany);

        repository.UpdatedCompanies.Should().ContainSingle().Which.Should().Be(existingCompany);
    }

    [Fact]
    public void Remove_WhenCompanyRemoved_DelegatesToRepository()
    {
        var existingCompany = TestDataFactory.CreateCompany(4);
        var repository = new FakeCompanyRepository(new[] { existingCompany });
        var service = new CompanyService(repository);

        service.Remove(existingCompany.CompanyId);

        repository.RemovedCompanyIds.Should().ContainSingle().Which.Should().Be(existingCompany.CompanyId);
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        private readonly List<Company> companies;

        public FakeCompanyRepository(IReadOnlyList<Company> companies)
        {
            this.companies = companies.ToList();
        }

        public List<Company> AddedCompanies { get; } = new List<Company>();
        public List<Company> UpdatedCompanies { get; } = new List<Company>();
        public List<int> RemovedCompanyIds { get; } = new List<int>();

        public Company? GetById(int companyId) => companies.FirstOrDefault(company => company.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => companies;
        public void Add(Company company) => AddedCompanies.Add(company);
        public void Update(Company company) => UpdatedCompanies.Add(company);
        public void Remove(int companyId) => RemovedCompanyIds.Add(companyId);
    }
}
