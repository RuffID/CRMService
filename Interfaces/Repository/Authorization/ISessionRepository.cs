using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface ISessionRepository : ICreateRepository<Session>, IDeleteRepository<Session>, IUpdateRepository<Session>
    {
        Task<IEnumerable<Session>?> GetAllItem(Range range);
        Task<Session?> GetItem(Session item, bool? trackable = null);
        Task DeleteByUserId(Guid userId);
        Task DeleteSessionsWithExpiredRefreshTokens();
        Task<int> GetCountOfItems();
    }
}
