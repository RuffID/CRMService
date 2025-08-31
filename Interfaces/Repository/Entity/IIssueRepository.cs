using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueRepository : IGetItemByIdRepository<Issue, int>, IUpsertItemByIdRepository<Issue, int>, ICreateItemRepository<Issue>
    {
        Task<List<Issue>> GetIssuesBetweenUpdateDates(DateTime dateFrom, DateTime dateTo, int startIndex, CancellationToken ct);
    }
}