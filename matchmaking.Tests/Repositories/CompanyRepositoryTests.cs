using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Tests;

[TestFixture]
public class CompanyRepositoryTests
{
    private CompanyRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new CompanyRepository();
    }

    [Test]
    public void GetById_ExistingCompanyId_ReturnsCompany()
    {
        var result = _repository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CompanyId, Is.EqualTo(1));
    }

    [Test]
    public void GetById_MissingCompanyId_ReturnsNull()
    {
        var result = _repository.GetById(-1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAll_WhenCalled_ReturnsAllCompanies()
    {
        var result = _repository.GetAll();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(10));
    }

    [Test]
    public void Add_NewCompany_AddsCompanyToRepository()
    {
        var newCompany = CreateCompany(1000);

        _repository.Add(newCompany);
        var result = _repository.GetById(1000);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CompanyName, Is.EqualTo("Test Company"));
    }

    [Test]
    public void Add_DuplicateCompanyId_ThrowsInvalidOperationException()
    {
        var duplicateCompany = CreateCompany(1);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(duplicateCompany));
    }

    [Test]
    public void Update_ExistingCompany_UpdatesStoredCompany()
    {
        var updatedCompany = CreateCompany(1);
        updatedCompany.CompanyName = "Updated Company Name";
        updatedCompany.Email = "updated.company@mail.com";
        updatedCompany.Phone = "0319999999";

        _repository.Update(updatedCompany);
        var result = _repository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CompanyName, Is.EqualTo("Updated Company Name"));
        Assert.That(result.Email, Is.EqualTo("updated.company@mail.com"));
        Assert.That(result.Phone, Is.EqualTo("0319999999"));
    }

    [Test]
    public void Update_MissingCompany_ThrowsKeyNotFoundException()
    {
        var missingCompany = CreateCompany(9999);

        Assert.Throws<KeyNotFoundException>(() => _repository.Update(missingCompany));
    }

    [Test]
    public void Remove_ExistingCompany_RemovesCompanyFromRepository()
    {
        _repository.Remove(1);
        var result = _repository.GetById(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Remove_MissingCompany_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Remove(9999));
    }

    private static Company CreateCompany(int companyId)
    {
        return new Company
        {
            CompanyId = companyId,
            CompanyName = "Test Company",
            Email = "test.company@mail.com",
            Phone = "0311234567"
        };
    }
}
