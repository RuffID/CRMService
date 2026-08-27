using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

[Collection(PostgreSqlCollection.Name)]
[Trait("Dependency", "Docker")]
public class PostgreSqlInfrastructureTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task EnsureCreatedAsync_CleanDatabase_CreatesCloudSchemaWithoutMigrations()
    {
        await using OkdeskContext context = fixture.CreateContext();

        Assert.Empty(context.Database.GetMigrations());
        Assert.True(await context.Database.CanConnectAsync(TestContext.Current.CancellationToken));
        Assert.True(await context.Companies.AnyAsync(TestContext.Current.CancellationToken) is false);
    }

    [Fact]
    public async Task CompanyDirectorySource_DiscriminatorAndIncludes_AreReadOnly()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = OkdeskTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OkdeskContext context = scope.ServiceProvider.GetRequiredService<OkdeskContext>();
        IOkdeskCompanyDirectorySource source = scope.ServiceProvider.GetRequiredService<IOkdeskCompanyDirectorySource>();
        CompanyCategory category = new() { Id = 7, Code = "vip", Name = "VIP", Color = "red" };
        Company company = new() { Id = 101, Name = "Company", Active = true, CategoryId = category.Id };
        Employee employee = new() { Id = 201, FirstName = "Employee", Active = true };
        Employee contact = new() { Id = 202, FirstName = "Contact", Active = true };
        context.AddRange(category, company);
        context.Entry(company).Property("InternalId").CurrentValue = 1001;
        context.Add(employee);
        context.Entry(employee).Property("InternalId").CurrentValue = 2001;
        context.Entry(employee).Property("Type").CurrentValue = "Employee";
        context.Add(contact);
        context.Entry(contact).Property("InternalId").CurrentValue = 2002;
        context.Entry(contact).Property("Type").CurrentValue = "Contact";
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        List<Employee> employees = await source.Employee.GetEmployeesReadOnlyAsync(TestContext.Current.CancellationToken);
        List<Employee> contacts = await source.Employee.GetContactsByIdsReadOnlyAsync([202], TestContext.Current.CancellationToken);
        List<Company> companies = await source.Company.GetAllWithCategoryReadOnlyAsync(TestContext.Current.CancellationToken);

        Assert.Equal(201, Assert.Single(employees).Id);
        Assert.Equal(202, Assert.Single(contacts).Id);
        Assert.Equal("vip", Assert.Single(companies).Category!.Code);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task EquipmentSource_FilterSortPaginationAndJsonMapping_ReturnExpectedRows()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = OkdeskTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OkdeskContext context = scope.ServiceProvider.GetRequiredService<OkdeskContext>();
        IOkdeskEquipmentSource source = scope.ServiceProvider.GetRequiredService<IOkdeskEquipmentSource>();
        Equipment first = new() { Id = 1, SerialNumber = "one" };
        Equipment second = new() { Id = 2, SerialNumber = "two" };
        Equipment third = new() { Id = 3, SerialNumber = "three" };
        context.AddRange(third, first, second);
        context.Entry(first).Property("ParametersJson").CurrentValue = "{\"code\":\"value\",\"empty\":null}";
        context.Entry(second).Property("ParametersJson").CurrentValue = "[1,2]";
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        List<Equipment> page = await source.Equipment.GetSyncItemsAsync(0, 2, TestContext.Current.CancellationToken);
        List<Equipment> next = await source.Equipment.GetSyncItemsAsync(2, 2, TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], page.Select(x => x.Id));
        Assert.Equal("value", Assert.Single(page[0].Parameters).Value);
        Assert.Equal("[1,2]", Assert.Single(page[1].Parameters).Value);
        Assert.Equal(3, Assert.Single(next).Id);
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
