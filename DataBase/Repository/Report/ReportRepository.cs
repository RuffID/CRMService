using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Report;
using CRMService.Models.Entity;
using CRMService.Models.Report;
using Microsoft.EntityFrameworkCore;

namespace CRMService.DataBase.Repository.Report
{
    public class ReportRepository(IQueryRepository<Issue> issues, IQueryRepository<TimeEntry> timeEntries) : IReportRepository
    {
        public async Task<IssueInfo[]> GetInfoForOpenIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken ct)
        {
            return await issues.Query(asNoTracking: true)
                .Where(i => i.AssigneeId == employeeId
                    && i.DeletedAt == null)
                .Where(i => !i.Status!.Code.Equals("completed")
                    && !i.Status.Code.Equals("closed"))
                .OrderBy(i => i.Id)
                .Select(i => new IssueInfo
                {
                    Id = i.Id, 
                    StatusId = i.StatusId, 
                    PriorityId = i.PriorityId, 
                    TypeId = i.TypeId
                }).ToArrayAsync(ct);
        }

        public async Task<long> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken ct)
        {
            return await issues.Query(asNoTracking: true)
                .Where(i => i.CompletedAt > dateFrom && i.CompletedAt < dateTo)
                .Where(i => i.AssigneeId == employeeId && i.DeletedAt == null)
                .Where(i => i.Status!.Code == "completed" || i.Status!.Code == "closed")
                .LongCountAsync(ct);
        }

        public async Task<IssueInfo[]> GetArraySolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken ct)
        {
                return await issues.Query(asNoTracking: true)
                    .Where(i => i.CompletedAt >= dateFrom && i.CompletedAt <= dateTo)
                    .Where(i => i.AssigneeId == employeeId && i.DeletedAt == null)
                    .Where(i => i.Status!.Code == "completed" || i.Status!.Code == "closed")
                    .Select(i => new IssueInfo
                    {
                        Id = i.Id,
                        StatusId = i.StatusId,
                        PriorityId = i.PriorityId,
                        TypeId = i.TypeId
                    }).ToArrayAsync(ct);
        }

        public async Task<double> GetSpentedTimeByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken ct)
        {
            return await timeEntries.Query(asNoTracking: true)
                .Where(te => te.LoggedAt > dateFrom && te.LoggedAt < dateTo && te.EmployeeId == employeeId)
                .Select(t => t.SpentTime)
                .SumAsync(ct);
        }
    }
}
