using CRMService.Application.Abstractions.Database.Repository.Authorization;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.Authorization
{
    public class SessionRepository(
        IGetItemByIdRepository<Session, Guid, MainContext> getItemById,
        IGetItemByPredicateRepository<Session, MainContext> getItemByPredicate,
        ICreateItemRepository<Session, MainContext> create,
        IDeleteItemRepository<Session, MainContext> delete
    ) : ISessionRepository
    {
        public Task<Session?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<Session>, IQueryable<Session>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Session?> GetItemByPredicateAsync(Expression<Func<Session, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Session>, IQueryable<Session>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Session>> GetItemsByPredicateAsync(Expression<Func<Session, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Session>, IQueryable<Session>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Session item)
            => create.Create(item);

        public void CreateRange(IEnumerable<Session> entities)
            => create.CreateRange(entities);

        public void Delete(Session item)
            => delete.Delete(item);

        public void DeleteRange(IEnumerable<Session> entities)
            => delete.DeleteRange(entities);
    }
}