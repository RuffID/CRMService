using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Models.OkdeskSource;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class KindParamServiceTests
{
    [Fact]
    public async Task UpsertConnections_ExternalParameterId_UsesLocalParameterId()
    {
        IEquipmentUnitOfWork unitOfWork = Substitute.For<IEquipmentUnitOfWork>();
        IOkdeskEquipmentSource source = Substitute.For<IOkdeskEquipmentSource>();
        IKindParameterRepository parameterRepository = Substitute.For<IKindParameterRepository>();
        IKindRepository kindRepository = Substitute.For<IKindRepository>();
        IKindParamsRepository connectionRepository = Substitute.For<IKindParamsRepository>();
        IOkdeskKindParamsRepository sourceConnectionRepository = Substitute.For<IOkdeskKindParamsRepository>();
        unitOfWork.KindParameter.Returns(parameterRepository);
        unitOfWork.Kind.Returns(kindRepository);
        unitOfWork.KindParams.Returns(connectionRepository);
        source.KindParams.Returns(sourceConnectionRepository);
        sourceConnectionRepository.GetAllReadOnlyAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<OkdeskKindParameterConnectionRecord>
            {
                new() { KindId = 6, KindParameterId = 10503 }
            }));
        parameterRepository.GetByOkdeskIdsReadOnlyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<KindsParameter>
            {
                new("ESM", null, EquipmentParameterFieldType.Date, 10503) { Id = 77 }
            }));
        kindRepository.GetByIdsReadOnlyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Kind> { new() { Id = 6 } }));
        connectionRepository.GetByKindIdsReadOnlyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<KindParam>()));
        KindParamService service = new(unitOfWork, source, Substitute.For<ILogger<KindParamService>>());

        await service.UpsertConnectionsFromCloudDb(TestContext.Current.CancellationToken);

        connectionRepository.Received(1).CreateRange(Arg.Is<IEnumerable<KindParam>>(items =>
            items.Single().KindId == 6 && items.Single().KindParameterId == 77));
        await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
