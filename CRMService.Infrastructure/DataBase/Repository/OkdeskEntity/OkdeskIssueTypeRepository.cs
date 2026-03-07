using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskIssueTypeRepository(
        IGetItemByIdRepository<IssueType, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<IssueType, OkdeskContext> getItemByPredicate) : IOkdeskIssueTypeRepository
    {
        public Task<IssueType?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssueType>, IQueryable<IssueType>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssueType?> GetItemByPredicateAsync(Expression<Func<IssueType, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssueType>, IQueryable<IssueType>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssueType>> GetItemsByPredicateAsync(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssueType>, IQueryable<IssueType>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}