using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IModelRepository :
        IGetItemByIdRepository<Model, int>,
        IGetItemsRepository<Model>,
        ICreateItemRepository<Model>
    {
        Task<Model?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<Model>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? kindIds, IReadOnlyCollection<int>? manufacturerIds, CancellationToken ct = default);
        Task<List<Model>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
