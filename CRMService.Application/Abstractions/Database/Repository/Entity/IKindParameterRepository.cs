using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IKindParameterRepository :
        IGetItemByIdRepository<KindsParameter, int>,
        IGetItemsRepository<KindsParameter>,
        ICreateItemRepository<KindsParameter>
    {
        Task<KindsParameter?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<KindsParameter>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
        Task<List<KindsParameter>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
