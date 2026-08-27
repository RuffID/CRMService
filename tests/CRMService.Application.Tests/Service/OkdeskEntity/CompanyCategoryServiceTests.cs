using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class CompanyCategoryServiceTests
{
    [Fact]
    public async Task CreateCategoryAsync_NewCategory_CreatesAndSaves()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        CompanyCategory category = new() { Id = 5, Code = "client", Name = "Client" };
        unitOfWork.CompanyCategory.GetItemByIdReadOnlyAsync(5, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompanyCategory?>(null));
        CompanyCategoryService service = CreateService(unitOfWork);

        bool result = await service.CreateCategoryAsync(category, CancellationToken.None);

        Assert.True(result);
        unitOfWork.CompanyCategory.Received(1).Create(category);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCategoryAsync_ExistingCategory_ReturnsFalseWithoutSaving()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        CompanyCategory category = new() { Id = 5 };
        unitOfWork.CompanyCategory.GetItemByIdReadOnlyAsync(5, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompanyCategory?>(category));
        CompanyCategoryService service = CreateService(unitOfWork);

        bool result = await service.CreateCategoryAsync(category, CancellationToken.None);

        Assert.False(result);
        unitOfWork.CompanyCategory.DidNotReceive().Create(Arg.Any<CompanyCategory>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCategoryAsync_ExistingCategory_CopiesStateAndSaves()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        CompanyCategory existing = new() { Id = 5, Code = "old", Name = "Old", Color = "#000000" };
        CompanyCategory updated = new() { Id = 5, Code = "new", Name = "New", Color = "#FFFFFF" };
        unitOfWork.CompanyCategory.GetItemByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompanyCategory?>(existing));
        CompanyCategoryService service = CreateService(unitOfWork);

        bool result = await service.UpdateCategoryAsync(updated, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("new", existing.Code);
        Assert.Equal("New", existing.Name);
        Assert.Equal("#FFFFFF", existing.Color);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckAnonymousCategory_MissingCategory_CreatesOnceAndSaves()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        unitOfWork.CompanyCategory.GetByCodeReadOnlyAsync("no_category", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompanyCategory?>(null));
        CompanyCategory? created = null;
        unitOfWork.CompanyCategory.When(repository => repository.Create(Arg.Any<CompanyCategory>()))
            .Do(call => created = call.Arg<CompanyCategory>());
        CompanyCategoryService service = CreateService(unitOfWork);

        await service.CheckAnonymousCategory(CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal("no_category", created.Code);
        Assert.Equal("Без категории", created.Name);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCategoriesAsync_RepositoryCancels_PropagatesCancellation()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        unitOfWork.CompanyCategory.GetItemsReadOnlyAsync(cancellation.Token)
            .Returns(Task.FromCanceled<List<CompanyCategory>>(cancellation.Token));
        CompanyCategoryService service = CreateService(unitOfWork);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetCategoriesAsync(cancellation.Token));
    }

    private static CompanyCategoryService CreateService(ICompanyDirectoryUnitOfWork unitOfWork)
    {
        return new CompanyCategoryService(
            Substitute.For<IOkdeskCompanyDirectorySource>(),
            unitOfWork,
            new EntitySyncService(),
            Substitute.For<ILogger<CompanyCategoryService>>());
    }
}
