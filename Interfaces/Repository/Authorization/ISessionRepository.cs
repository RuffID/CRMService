using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Auth
{
    public interface ISessionRepository : IGetRepository<Session>, ICreateRepository<Session>, IDeleteRepository<Session>, IUpdateRepository<Session>
    {
        Task DeleteByUserId(Guid userId);
        Task DeleteSessionsWithExpiredRefreshTokens();
        Task<int> GetCountOfItems();
    }
}
