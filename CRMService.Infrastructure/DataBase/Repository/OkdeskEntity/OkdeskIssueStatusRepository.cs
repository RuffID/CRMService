using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskIssueStatusRepository(
        IGetItemByIdRepository<IssueStatus, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<IssueStatus, OkdeskContext> getItemByPredicate) : IOkdeskIssueStatusRepository
    {
        public Task<IssueStatus?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssueStatus>, IQueryable<IssueStatus>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssueStatus?> GetItemByPredicateAsync(Expression<Func<IssueStatus, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssueStatus>, IQueryable<IssueStatus>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssueStatus>> GetItemsByPredicateAsync(Expression<Func<IssueStatus, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssueStatus>, IQueryable<IssueStatus>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}