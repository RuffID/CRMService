using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueTypeGroupRepository :
        IGetItemByIdRepository<IssueTypeGroup, int>,
        IGetItemsRepository<IssueTypeGroup>,
        ICreateItemRepository<IssueTypeGroup>
    {
        Task<IssueTypeGroup?> GetByCodeAsync(string code, CancellationToken ct = default);
    }
}
