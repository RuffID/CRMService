using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Application.Service.Webhook;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.Webhook;

public class CompanyWebhookServiceTests
{
    [Fact]
    public async Task HandleWebhook_MissingCompany_ReturnsFalseWithoutSaving()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        CompanyWebhookService service = CreateService(unitOfWork);

        bool result = await service.HandleWebhook(
            new RootEventWebHook { Event = new EventWebHook { Event_type = "new_company" } },
            CancellationToken.None);

        Assert.False(result);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWebhook_UnsupportedEvent_ReturnsFalseWithoutSaving()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        CompanyWebhookService service = CreateService(unitOfWork);

        bool result = await service.HandleWebhook(
            new RootEventWebHook
            {
                Event = new EventWebHook { Event_type = "unrelated" },
                Company = new Company { Id = 10 }
            },
            CancellationToken.None);

        Assert.False(result);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWebhook_RepeatedCompanyEvent_DoesNotCreateDuplicate()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        Company incoming = new() { Id = 10, Name = "Updated", Active = true };
        Company existing = new() { Id = 10, Name = "Old", Active = false };
        unitOfWork.Company.GetItemByIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Company?>(null),
                Task.FromResult<Company?>(existing));
        CompanyWebhookService service = CreateService(unitOfWork);
        RootEventWebHook @event = new()
        {
            Event = new EventWebHook { Event_type = "change_company" },
            Company = incoming
        };

        Assert.True(await service.HandleWebhook(@event, CancellationToken.None));
        Assert.True(await service.HandleWebhook(@event, CancellationToken.None));

        unitOfWork.Company.Received(1).Create(incoming);
        Assert.Equal("Updated", existing.Name);
        Assert.True(existing.Active);
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWebhook_RepositoryCancellation_PropagatesCancellation()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        unitOfWork.Company.GetItemByIdAsync(10, cancellation.Token)
            .Returns(Task.FromCanceled<Company?>(cancellation.Token));
        CompanyWebhookService service = CreateService(unitOfWork);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.HandleWebhook(
            new RootEventWebHook
            {
                Event = new EventWebHook { Event_type = "new_company" },
                Company = new Company { Id = 10 }
            },
            cancellation.Token));
    }

    private static CompanyWebhookService CreateService(ICompanyDirectoryUnitOfWork unitOfWork)
    {
        EntitySyncService sync = new();
        CompanyService companyService = new(
            Options.Create(new ApiEndpointOptions()),
            Options.Create(new OkdeskOptions()),
            Substitute.For<IOkdeskEntityRequestService>(),
            unitOfWork,
            Substitute.For<IOkdeskCompanyDirectorySource>(),
            sync,
            Substitute.For<ILogger<CompanyService>>());

        return new CompanyWebhookService(
            companyService,
            sync,
            Substitute.For<ILogger<CompanyWebhookService>>());
    }
}
