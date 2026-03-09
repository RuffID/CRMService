using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Report
{
    public class EmployeePerformanceReportRepository(
        IQueryRepository<Issue, MainContext> issues,
        IQueryRepository<TimeEntry, MainContext> timeEntries) : IEmployeePerformanceReportRepository
    {
        public async Task<List<SolvedIssuesCountInfo>> GetSolvedIssuesCountByEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct)
        {
            IQueryable<Issue> query = issues.Query(asNoTracking: true);

            query = ApplyIssueFilters(query, filters);
            query = query
                .Where(i => i.AssigneeId != null)
                .Where(i => i.CompletedAt > dateFrom && i.CompletedAt < dateTo)
                .Where(i => i.Status!.Code == "completed" || i.Status!.Code == "closed");

            return await query
                .GroupBy(i => i.AssigneeId!.Value)
                .Select(g => new SolvedIssuesCountInfo
                {
                    EmployeeId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);
        }

        public async Task<List<IssueInfo>> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct)
        {
            IQueryable<Issue> query = issues.Query(asNoTracking: true);

            query = ApplyIssueFilters(query, filters);
            query = query
                .Where(i => i.AssigneeId != null)
                .Where(i => i.CompletedAt >= dateFrom && i.CompletedAt <= dateTo)
                .Where(i => i.Status!.Code == "completed" || i.Status!.Code == "closed");

            return await query
                .Select(i => new IssueInfo
                {
                    EmployeeId = i.AssigneeId!.Value,
                    Id = i.Id,
                    StatusId = i.StatusId,
                    PriorityId = i.PriorityId,
                    TypeId = i.TypeId
                })
                .ToListAsync(ct);
        }

        public async Task<List<SpentedTimeInfo>> GetSpentedTimeByEmployee(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct)
        {
            IQueryable<TimeEntry> query = timeEntries.Query(asNoTracking: true)
                .Where(te => te.LoggedAt > dateFrom && te.LoggedAt < dateTo);

            if (filters?.HasEmployees == true)
                query = query.Where(te => filters.EmployeeIds!.Contains(te.EmployeeId));

            return await query
                .GroupBy(te => te.EmployeeId)
                .Select(g => new SpentedTimeInfo
                {
                    EmployeeId = g.Key,
                    SpentedTime = g.Sum(x => x.SpentTime)
                })
                .ToListAsync(ct);
        }

        public async Task<List<SolvedIssuesCountInfo>> GetOpenIssuesCountByEmployees(ReportRequest? filters, CancellationToken ct)
        {
            IQueryable<Issue> query = issues.Query(asNoTracking: true);

            query = ApplyIssueFilters(query, filters);
            query = query
                .Where(i => i.AssigneeId != null)
                .Where(i => !i.Status!.Code.Equals("completed") && !i.Status.Code.Equals("closed"));

            return await query
                .GroupBy(i => i.AssigneeId!.Value)
                .Select(g => new SolvedIssuesCountInfo
                {
                    EmployeeId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);
        }

        private static IQueryable<Issue> ApplyIssueFilters(IQueryable<Issue> query, ReportRequest? filters)
        {
            query = query.Where(i => i.DeletedAt == null);

            if (filters == null)
                return query;

            if (filters.HasEmployees)
                query = query.Where(i => i.AssigneeId != null && filters.EmployeeIds!.Contains(i.AssigneeId.Value));

            if (filters.HasStatus)
                query = query.Where(i => i.StatusId != null && filters.StatusIds!.Contains(i.StatusId.Value));

            if (filters.HasPriority)
                query = query.Where(i => i.PriorityId != null && filters.PriorityIds!.Contains(i.PriorityId.Value));

            if (filters.HasType)
                query = query.Where(i => i.TypeId != null && filters.TypeIds!.Contains(i.TypeId.Value));

            return query;
        }
    }
}