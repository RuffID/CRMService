using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueTypeRepository :
        IGetItemByIdRepository<IssueType, int>,
        IGetItemsRepository<IssueType>,
        ICreateItemRepository<IssueType>
    {
        Task<IssueType?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IssueType?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<IssueType>> GetAllWithGroupReadOnlyAsync(CancellationToken ct = default);
        Task<List<IssueType>> SearchReadOnlyAsync(string? search, CancellationToken ct = default);
        Task<List<IssueType>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
