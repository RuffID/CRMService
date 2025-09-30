using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IIssueRepository : IGetItemByIdRepository<Issue, int>, IUpsertItemByIdRepository<Issue, int>, ICreateItemRepository<Issue>
    {
        Task<List<Issue>> GetIssuesBetweenUpdateDates(DateTime dateFrom, DateTime dateTo, int startIndex, CancellationToken ct);
    }
}