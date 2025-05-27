using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueRepository : IGetRepository<Issue>, IUpdateRepository<Issue>, ICreateRepository<Issue>
    {
        Task<IEnumerable<Issue>?> GetIssuesBetweenUpdateDates(DateTime dateFrom, DateTime dateTo, int startIndex);
        Task CreateOrUpdate(IEnumerable<Issue> items);
        Task CreateOrUpdate(Issue item);
        Task<Issue?> GetIssueById(int id, bool? trackable = null);
    }
}