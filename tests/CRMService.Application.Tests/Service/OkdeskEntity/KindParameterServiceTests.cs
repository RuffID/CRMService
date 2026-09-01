using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Models.OkdeskApi;
using CRMService.Application.Models.OkdeskSource;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class KindParameterServiceTests
{
    [Fact]
    public async Task UpdateFromApi_MissingParameter_CreatesLocalParameterWithoutOkdeskId()
    {
        IEquipmentUnitOfWork unitOfWork = Substitute.For<IEquipmentUnitOfWork>();
        IKindParameterRepository repository = Substitute.For<IKindParameterRepository>();
        IOkdeskEntityRequestService request = Substitute.For<IOkdeskEntityRequestService>();
        unitOfWork.KindParameter.Returns(repository);
        request.GetRangeOfItemsAsync<EquipmentParameterSchema>(Arg.Any<string>(), ct: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<EquipmentParameterSchema>
            {
                new() { Code = "ESM", Name = "Лицензия ЕСМ до", FieldType = EquipmentParameterFieldType.Date }
            }));
        repository.GetByCodeAsync("ESM", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<KindsParameter?>(null));
        KindsParameter? created = null;
        repository.When(value => value.Create(Arg.Any<KindsParameter>()))
            .Do(call => created = call.Arg<KindsParameter>());
        KindParameterService service = CreateService(unitOfWork, request: request);

        await service.UpdateKindParametersFromCloudApi(TestContext.Current.CancellationToken);

        Assert.NotNull(created);
        Assert.Equal("ESM", created.Code);
        Assert.Null(created.OkdeskId);
        await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateFromCloudDb_ExistingCode_AttachesOkdeskIdWithoutCreatingDuplicate()
    {
        IEquipmentUnitOfWork unitOfWork = Substitute.For<IEquipmentUnitOfWork>();
        IKindParameterRepository repository = Substitute.For<IKindParameterRepository>();
        IOkdeskEquipmentSource source = Substitute.For<IOkdeskEquipmentSource>();
        IOkdeskKindParameterRepository sourceRepository = Substitute.For<IOkdeskKindParameterRepository>();
        unitOfWork.KindParameter.Returns(repository);
        source.KindParameter.Returns(sourceRepository);
        sourceRepository.GetAllReadOnlyAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<OkdeskKindParameterRecord>
            {
                new() { Id = 10503, Code = "ESM", Name = "Лицензия ЕСМ до", FieldType = EquipmentParameterFieldType.Date }
            }));
        KindsParameter existing = new("ESM", "Старое название", EquipmentParameterFieldType.String) { Id = 17 };
        repository.GetByOkdeskIdAsync(10503, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<KindsParameter?>(null));
        repository.GetByCodeAsync("ESM", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<KindsParameter?>(existing));
        KindParameterService service = CreateService(unitOfWork, source: source);

        await service.UpdateKindParametersFromCloudDb(TestContext.Current.CancellationToken);

        Assert.Equal(10503, existing.OkdeskId);
        Assert.Equal("Лицензия ЕСМ до", existing.Name);
        Assert.Equal(EquipmentParameterFieldType.Date, existing.FieldType);
        repository.DidNotReceive().Create(Arg.Any<KindsParameter>());
        await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateFromCloudDb_CodeAndOkdeskIdBelongToDifferentRows_ThrowsConflict()
    {
        IEquipmentUnitOfWork unitOfWork = Substitute.For<IEquipmentUnitOfWork>();
        IKindParameterRepository repository = Substitute.For<IKindParameterRepository>();
        IOkdeskEquipmentSource source = Substitute.For<IOkdeskEquipmentSource>();
        IOkdeskKindParameterRepository sourceRepository = Substitute.For<IOkdeskKindParameterRepository>();
        unitOfWork.KindParameter.Returns(repository);
        source.KindParameter.Returns(sourceRepository);
        sourceRepository.GetAllReadOnlyAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<OkdeskKindParameterRecord>
            {
                new() { Id = 10503, Code = "ESM" }
            }));
        repository.GetByOkdeskIdAsync(10503, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<KindsParameter?>(new KindsParameter("OTHER", null, null, 10503) { Id = 1 }));
        repository.GetByCodeAsync("ESM", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<KindsParameter?>(new KindsParameter("ESM", null, null) { Id = 2 }));
        KindParameterService service = CreateService(unitOfWork, source: source);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateKindParametersFromCloudDb(TestContext.Current.CancellationToken));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static KindParameterService CreateService(
        IEquipmentUnitOfWork unitOfWork,
        IOkdeskEntityRequestService? request = null,
        IOkdeskEquipmentSource? source = null)
    {
        return new KindParameterService(
            Options.Create(new ApiEndpointOptions { OkdeskDomainUrl = "https://okdesk.invalid" }),
            Options.Create(new OkdeskOptions { OkdeskApiToken = "token" }),
            request ?? Substitute.For<IOkdeskEntityRequestService>(),
            unitOfWork,
            source ?? Substitute.For<IOkdeskEquipmentSource>(),
            new EntitySyncService(),
            Substitute.For<ILogger<KindParameterService>>());
    }
}
