using CRMService.DataBase;
using CRMService.Interfaces.Repository.Report;
using CRMService.Models.Report;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Report
{
    public class ReportRepository(ApplicationContext context, ILoggerFactory logger) : IReportRepository
    {
        private readonly ILogger<ReportRepository> _logger = logger.CreateLogger<ReportRepository>();

        public async Task<IssueInfo[]?> GetInfoForOpenIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken)
        {
            try
            {
                IssueInfo[]? result = await (from issue in context.Issues
                                             join status in context.IssueStatuses on issue.StatusId equals status.Id into statusJoin
                                             from status in statusJoin.DefaultIfEmpty() // Left join
                                             where issue.AssigneeId == employeeId
                                             && issue.DeletedAt == null
                                             && (status == null || (status.Code != "completed" && status.Code != "closed"))
                                             orderby issue.Id
                                             select new IssueInfo()
                                             {
                                                 Id = issue.Id,
                                                 StatusId = issue.StatusId,
                                                 PriorityId = issue.PriorityId,
                                                 TypeId = issue.TypeId
                                             })
                              .ToArrayAsync(cancellationToken);

                return result?.Length == 0 ? null : result;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "The operation: {GetInfoForOpenIssuesByEmployee} was interrupted by the user.", nameof(GetSolvedIssuesByEmployee));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue info array by employee id.");
                return null;
            }
        }

        public async Task<long?> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await (from issue in context.Issues
                                   join status in context.IssueStatuses on issue.StatusId equals status.Id
                                   where issue.CompletedAt > dateFrom
                                   && issue.CompletedAt < dateTo
                                   && (status.Code == "completed" || status.Code == "closed")
                                   && issue.AssigneeId == employeeId
                                   && issue.DeletedAt == null
                                   select issue)
                             .CountAsync(cancellationToken);

                return count == 0 ? null : count;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "The operation: {MethodName} was interrupted by the user.", nameof(GetSolvedIssuesByEmployee));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count solved issues by employee id.");
                return null;
            }
        }

        public async Task<IssueInfo[]?> GetArraySolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken)
        {
            try
            {
                IssueInfo[]? result = await (from issue in context.Issues
                                             join status in context.IssueStatuses on issue.StatusId equals status.Id
                                             where issue.CompletedAt > dateFrom
                                             && issue.CompletedAt < dateTo
                                             && (status.Code == "completed" || status.Code == "closed")
                                             && issue.AssigneeId == employeeId
                                             && issue.DeletedAt == null
                                             select new IssueInfo()
                                             {
                                                 Id = issue.Id,
                                                 StatusId = issue.StatusId,
                                                 PriorityId = issue.PriorityId,
                                                 TypeId = issue.TypeId
                                             })
                              .ToArrayAsync(cancellationToken);

                return result;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "The operation: {MethodName} was interrupted by the user.", nameof(GetSolvedIssuesByEmployee));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count solved issues by employee id.");
                return null;
            }
        }

        public async Task<double?> GetSpentedTimeByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken)
        {
            try
            {
                double? summ = await (from time in context.TimeEntries
                                      where time.LoggedAt > dateFrom
                                      && time.LoggedAt < dateTo
                                      && time.EmployeeId == employeeId
                                      select time.SpentTime)
                             .SumAsync(cancellationToken);

                return summ == 0 ? null : summ;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "The operation: {MethodName} was interrupted by the user.", nameof(GetSpentedTimeByEmployee));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving spent time by employee id.");
                return null;
            }
        }
    }
}
