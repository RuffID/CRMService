using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskIssueRepository(
        IGetItemByIdRepository<Issue, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<Issue, OkdeskContext> getItemByPredicate,
        IQueryRepository<Issue, OkdeskContext> issueQuery,
        IQueryRepository<Employee, OkdeskContext> employeeQuery) : IOkdeskIssueRepository
    {
        public Task<Issue?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Issue?> GetItemByPredicateAsync(Expression<Func<Issue, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Issue>> GetItemsByPredicateAsync(Expression<Func<Issue, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public async Task<List<Issue>> GetUpdatedItemsAsync(DateTime dateFrom, DateTime dateTo, int startId, int limit, CancellationToken ct = default)
        {
            List<IssueSyncProjection> rows = await issueQuery.Query(true)
                .Where(x =>
                    (((x.EmployeesUpdatedAt >= dateFrom) && (x.EmployeesUpdatedAt <= dateTo)) ||
                    ((x.DeletedAt >= dateFrom) && (x.DeletedAt <= dateTo))) &&
                    x.Id > startId)
                .OrderBy(x => x.Id)
                .Take(limit)
                .Select(x => new IssueSyncProjection
                {
                    Id = x.Id,
                    AssigneeId = x.Assignee != null ? x.Assignee.Id : null,
                    AuthorId = employeeQuery.Query()
                        .Where(e => EF.Property<int>(e, "InternalId") == EF.Property<int?>(x, "AuthorInternalId"))
                        .Select(e => (int?)e.Id)
                        .FirstOrDefault(),
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    CompletedAt = x.CompletedAt,
                    DeadlineAt = x.DeadlineAt,
                    EmployeesUpdatedAt = x.EmployeesUpdatedAt,
                    DeletedAt = x.DeletedAt,
                    DelayTo = x.DelayTo,
                    StatusCode = x.Status != null ? x.Status.Code : null,
                    TypeCode = x.Type != null ? x.Type.Code : null,
                    PriorityCode = x.Priority != null ? x.Priority.Code : null,
                    CompanyId = x.Company != null ? x.Company.Id : null,
                    ServiceObjectId = x.ServiceObject != null ? x.ServiceObject.Id : null
                })
                .ToListAsync(ct);

            return rows
                .Select(x => new Issue
                {
                    Id = x.Id,
                    AssigneeId = x.AssigneeId,
                    AuthorId = x.AuthorId,
                    CompanyId = x.CompanyId,
                    ServiceObjectId = x.ServiceObjectId,
                    Title = x.Title ?? string.Empty,
                    Status = new IssueStatus { Code = x.StatusCode ?? string.Empty },
                    Type = new IssueType { Code = x.TypeCode ?? string.Empty },
                    Priority = new IssuePriority { Code = x.PriorityCode ?? string.Empty },
                    CreatedAt = x.CreatedAt.ToLocalTime(),
                    CompletedAt = x.CompletedAt?.ToLocalTime(),
                    DeadlineAt = x.DeadlineAt?.ToLocalTime(),
                    DelayTo = x.DelayTo?.ToLocalTime(),
                    EmployeesUpdatedAt = x.EmployeesUpdatedAt.ToLocalTime(),
                    DeletedAt = x.DeletedAt?.ToLocalTime()
                })
                .ToList();
        }

        private class IssueSyncProjection
        {
            public int Id { get; set; }
            public int? AssigneeId { get; set; }
            public int? AuthorId { get; set; }
            public string? Title { get; set; }
            public DateTime EmployeesUpdatedAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime? DeadlineAt { get; set; }
            public DateTime? DelayTo { get; set; }
            public DateTime? DeletedAt { get; set; }
            public string? StatusCode { get; set; }
            public string? TypeCode { get; set; }
            public string? PriorityCode { get; set; }
            public int? CompanyId { get; set; }
            public int? ServiceObjectId { get; set; }
        }
    }
}