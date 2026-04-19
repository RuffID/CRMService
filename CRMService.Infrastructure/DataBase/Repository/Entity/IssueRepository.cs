using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Contracts.Models.Request;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class IssueRepository(
        IGetItemByIdRepository<Issue, int, MainContext> getItemById,
        IGetItemByPredicateRepository<Issue, MainContext> getItemByPredicate,
        ICreateItemRepository<Issue, MainContext> create,
        IQueryRepository<Issue, MainContext> query
    ) : IIssueRepository
    {
        public Task<Issue?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Issue?> GetItemByPredicateAsync(Expression<Func<Issue, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Issue>> GetItemsByPredicateAsync(Expression<Func<Issue, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Issue item)
            => create.Create(item);

        public void CreateRange(IEnumerable<Issue> entities)
            => create.CreateRange(entities);

        public async Task<int> GetCountByFilterAsync(IssueListRequest request, int? maxCount, CancellationToken ct)
        {
            IQueryable<Issue> items = ApplyFilters(query.Query(asNoTracking: true), request);

            if (maxCount.HasValue)
                return await items
                    .OrderByDescending(issue => issue.CreatedAt)
                    .ThenByDescending(issue => issue.Id)
                    .Take(maxCount.Value)
                    .CountAsync(ct);

            return await items.CountAsync(ct);
        }

        public Task<List<Issue>> GetPageByFilterAsync(IssueListRequest request, int skip, int take, CancellationToken ct)
        {
            IQueryable<Issue> items = ApplyFilters(query.Query(asNoTracking: true), request)
                .Include(issue => issue.Company)
                .ThenInclude(company => company!.Category)
                .Include(issue => issue.Assignee)
                .Include(issue => issue.Priority)
                .Include(issue => issue.Status)
                .OrderByDescending(issue => issue.CreatedAt)
                .ThenByDescending(issue => issue.Id)
                .Skip(skip)
                .Take(take);

            return items.ToListAsync(ct);
        }

        private static IQueryable<Issue> ApplyFilters(IQueryable<Issue> query, IssueListRequest request)
        {
            query = query.Where(issue => issue.DeletedAt == null);

            if (request.NumberFrom.HasValue)
                query = query.Where(issue => issue.Id >= request.NumberFrom.Value);

            if (request.NumberTo.HasValue)
                query = query.Where(issue => issue.Id <= request.NumberTo.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim();
                bool isIssueNumber = int.TryParse(search, out int issueNumber);

                query = query.Where(issue =>
                    (issue.Title != null && issue.Title.Contains(search))
                    || (isIssueNumber && issue.Id == issueNumber));
            }

            if (request.AssigneeIds != null && request.AssigneeIds.Count > 0)
                query = query.Where(issue => issue.AssigneeId.HasValue && request.AssigneeIds.Contains(issue.AssigneeId.Value));

            if (request.AuthorIds != null && request.AuthorIds.Count > 0)
                query = query.Where(issue => issue.AuthorId.HasValue && request.AuthorIds.Contains(issue.AuthorId.Value));

            if (request.TypeIds != null && request.TypeIds.Count > 0)
                query = query.Where(issue => issue.TypeId.HasValue && request.TypeIds.Contains(issue.TypeId.Value));

            if (request.StatusIds != null && request.StatusIds.Count > 0)
                query = query.Where(issue => issue.StatusId.HasValue && request.StatusIds.Contains(issue.StatusId.Value));

            if (request.PriorityIds != null && request.PriorityIds.Count > 0)
                query = query.Where(issue => issue.PriorityId.HasValue && request.PriorityIds.Contains(issue.PriorityId.Value));

            if (request.CompanyIds != null && request.CompanyIds.Count > 0)
                query = query.Where(issue => issue.CompanyId.HasValue && request.CompanyIds.Contains(issue.CompanyId.Value));

            if (request.GroupIds != null && request.GroupIds.Count > 0)
                query = query.Where(issue =>
                    issue.Assignee != null &&
                    issue.Assignee.EmployeeGroups.Any(employeeGroup => request.GroupIds.Contains(employeeGroup.GroupId)));

            if (request.RegistrationDateFrom.HasValue)
                query = query.Where(issue => issue.CreatedAt >= request.RegistrationDateFrom.Value);

            if (request.RegistrationDateTo.HasValue)
                query = query.Where(issue => issue.CreatedAt < request.RegistrationDateTo.Value);

            if (request.ResolutionDateFrom.HasValue)
                query = query.Where(issue => issue.CompletedAt.HasValue && issue.CompletedAt.Value >= request.ResolutionDateFrom.Value);

            if (request.ResolutionDateTo.HasValue)
                query = query.Where(issue => issue.CompletedAt.HasValue && issue.CompletedAt.Value < request.ResolutionDateTo.Value);

            return query;
        }
    }
}
