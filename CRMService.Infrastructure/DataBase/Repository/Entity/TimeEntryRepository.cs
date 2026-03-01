using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class TimeEntryRepository(IGetItemByIdRepository<TimeEntry, int> getItemById,
        IGetItemByPredicateRepository<TimeEntry> getItemByPredicate,
        ICreateItemRepository<TimeEntry> create,
        IDeleteItemRepository<TimeEntry> delete) : ITimeEntryRepository
    {
        public Task<TimeEntry?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<TimeEntry?> GetItemByPredicateAsync(Expression<Func<TimeEntry, bool>> predicate, bool asNoTracking = false, Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include,  ct);

        public Task<List<TimeEntry>> GetItemsByPredicateAsync(Expression<Func<TimeEntry, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(TimeEntry item) => create.Create(item);

        public void CreateRange(IEnumerable<TimeEntry> entities) => create.CreateRange(entities);

        public void Delete(TimeEntry item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<TimeEntry> items) => delete.DeleteRange(items);
    }
}



