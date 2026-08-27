using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface ISessionRepository :
        IGetItemByIdRepository<Session, Guid>,
        ICreateItemRepository<Session>,
        IDeleteItemRepository<Session>
    {
        Task<Session?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}
