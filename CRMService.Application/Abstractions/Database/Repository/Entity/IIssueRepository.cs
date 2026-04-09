using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueRepository :
        IGetItemByIdRepository<Issue, int, DbContext>,
        IGetItemByPredicateRepository<Issue, DbContext>,
        ICreateItemRepository<Issue, DbContext>
    {
        Task<int> GetCountByFilterAsync(IssueListRequest request, int? maxCount, CancellationToken ct);
        Task<List<Issue>> GetPageByFilterAsync(IssueListRequest request, int skip, int take, CancellationToken ct);
    }
}
