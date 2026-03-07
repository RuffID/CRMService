using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskTimeEntryRepository(
        IGetItemByIdRepository<TimeEntry, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<TimeEntry, OkdeskContext> getItemByPredicate,
        IQueryRepository<TimeEntry, OkdeskContext> query) : IOkdeskTimeEntryRepository
    {
        public Task<TimeEntry?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<TimeEntry?> GetItemByPredicateAsync(Expression<Func<TimeEntry, bool>> predicate, bool asNoTracking = false, Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<TimeEntry>> GetItemsByPredicateAsync(Expression<Func<TimeEntry, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public async Task<List<TimeEntry>> GetLoggedItemsAsync(DateTime dateFrom, DateTime dateTo, long startId, long limit, CancellationToken ct = default)
        {
            List<TimeEntrySyncProjection> rows = await query.Query(true)
                .Where(x => x.LoggedAt >= dateFrom && x.LoggedAt <= dateTo && x.Id > startId)
                .OrderBy(x => x.Id)
                .Take((int)limit)
                .Select(x => new TimeEntrySyncProjection
                {
                    Id = x.Id,
                    EmployeeId = x.Employee != null ? x.Employee.Id : 0,
                    IssueId = x.Issue != null ? x.Issue.Id : 0,
                    SpentTime = x.SpentTime,
                    LoggedAt = x.LoggedAt,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            return rows
                .Select(x => new TimeEntry
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    IssueId = x.IssueId,
                    SpentTime = x.SpentTime,
                    LoggedAt = x.LoggedAt.ToLocalTime(),
                    CreatedAt = x.CreatedAt?.ToLocalTime()
                })
                .ToList();
        }

        private class TimeEntrySyncProjection
        {
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public int IssueId { get; set; }
            public double SpentTime { get; set; }
            public DateTime LoggedAt { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}