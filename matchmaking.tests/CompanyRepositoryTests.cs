using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class CompanyRepositoryTests
{
    [Fact]
    public void GetById_AddedCompanyId_ReturnsCompany()
    {
        var company = CreateCompany(1000);
        var repository = CreateRepositoryWith(company);

        var result = repository.GetById(company.CompanyId);

        result.Should().NotBeNull();
        result!.CompanyId.Should().Be(company.CompanyId);
        result.CompanyName.Should().Be(company.CompanyName);
    }

    [Fact]
    public void GetById_MissingCompanyId_ReturnsNull()
    {
        var repository = CreateRepositoryWith();

        var result = repository.GetById(-1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenCompanyAdded_ReturnsAddedCompany()
    {
        var company = CreateCompany(1000);
        var repository = CreateRepositoryWith(company);

        var result = repository.GetAll();

        result.Should().ContainSingle(item => item.CompanyId == company.CompanyId);
    }

    [Fact]
    public void Add_NewCompany_AddsCompanyToRepository()
    {
        var repository = CreateRepositoryWith();
        var newCompany = CreateCompany(1000);

        repository.Add(newCompany);
        var result = repository.GetById(newCompany.CompanyId);

        result.Should().NotBeNull();
        result!.CompanyName.Should().Be(newCompany.CompanyName);
    }

    [Fact]
    public void Add_DuplicateCompanyId_ThrowsInvalidOperationException()
    {
        var existingCompany = CreateCompany(1000);
        var repository = CreateRepositoryWith(existingCompany);
        var duplicateCompany = CreateCompany(existingCompany.CompanyId);

        Action act = () => repository.Add(duplicateCompany);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingCompany_UpdatesStoredCompany()
    {
        var existingCompany = CreateCompany(1000);
        var repository = CreateRepositoryWith(existingCompany);
        var updatedCompany = CreateCompany(existingCompany.CompanyId);
        updatedCompany.CompanyName = "Updated Company Name";
        updatedCompany.Email = "updated.company@mail.com";
        updatedCompany.Phone = "0319999999";

        repository.Update(updatedCompany);
        var result = repository.GetById(updatedCompany.CompanyId);

        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Updated Company Name");
        result.Email.Should().Be("updated.company@mail.com");
        result.Phone.Should().Be("0319999999");
    }

    [Fact]
    public void Update_MissingCompany_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();
        var missingCompany = CreateCompany(9999);

        Action act = () => repository.Update(missingCompany);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingCompany_RemovesCompanyFromRepository()
    {
        var company = CreateCompany(1000);
        var repository = CreateRepositoryWith(company);

        repository.Remove(company.CompanyId);
        var result = repository.GetById(company.CompanyId);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingCompany_ThrowsKeyNotFoundException()
    {
        var repository = CreateRepositoryWith();

        Action act = () => repository.Remove(9999);

        act.Should().Throw<KeyNotFoundException>();
    }

    private static CompanyRepository CreateRepositoryWith(params Company[] companies)
    {
        var repository = new CompanyRepository([]);
        foreach (var company in companies)
        {
            repository.Add(company);
        }

        return repository;
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
