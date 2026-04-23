using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using FluentAssertions;
using Xunit;

namespace matchmaking.Tests;

public class CompanyRepositoryTests
{
    private readonly CompanyRepository repository = new ();

    [Fact]
    public void GetById_ExistingCompanyId_ReturnsCompany()
    {
        var result = repository.GetById(1);

        result.Should().NotBeNull();
        result!.CompanyId.Should().Be(1);
    }

    [Fact]
    public void GetById_MissingCompanyId_ReturnsNull()
    {
        var result = repository.GetById(-1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WhenCalled_ReturnsAllCompanies()
    {
        var result = repository.GetAll();

        result.Should().HaveCount(10);
    }

    [Fact]
    public void Add_NewCompany_AddsCompanyToRepository()
    {
        var newCompany = CreateCompany(1000);

        repository.Add(newCompany);
        var result = repository.GetById(1000);

        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public void Add_DuplicateCompanyId_ThrowsInvalidOperationException()
    {
        var duplicateCompany = CreateCompany(1);

        Action act = () => repository.Add(duplicateCompany);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_ExistingCompany_UpdatesStoredCompany()
    {
        var updatedCompany = CreateCompany(1);
        updatedCompany.CompanyName = "Updated Company Name";
        updatedCompany.Email = "updated.company@mail.com";
        updatedCompany.Phone = "0319999999";

        repository.Update(updatedCompany);
        var result = repository.GetById(1);

        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Updated Company Name");
        result.Email.Should().Be("updated.company@mail.com");
        result.Phone.Should().Be("0319999999");
    }

    [Fact]
    public void Update_MissingCompany_ThrowsKeyNotFoundException()
    {
        var missingCompany = CreateCompany(9999);

        Action act = () => repository.Update(missingCompany);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Remove_ExistingCompany_RemovesCompanyFromRepository()
    {
        repository.Remove(1);
        var result = repository.GetById(1);

        result.Should().BeNull();
    }

    [Fact]
    public void Remove_MissingCompany_ThrowsKeyNotFoundException()
    {
        Action act = () => repository.Remove(9999);

        act.Should().Throw<KeyNotFoundException>();
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
