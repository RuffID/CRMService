using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Report
{
    public class ReportRepository(
        IQueryRepository<Issue, MainContext> issues,
        IQueryRepository<TimeEntry, MainContext> timeEntries,
        IQueryRepository<EmployeeGroup, MainContext> employeeGroups) : IReportRepository
    {
        public async Task<List<IssueInfo>> GetInfoForOpenIssuesByEmployee(ReportRequest? filters, CancellationToken ct)
        {
            IQueryable<Issue> query = issues.Query(asNoTracking: true);

            query = ApplyIssueFilters(query, filters);
            query = query.Where(i => !i.Status!.Code.Equals("completed") && !i.Status.Code.Equals("closed"));

            List<IssueInfo> result = await query
                .Where(i => i.AssigneeId != null)
                .Select(i => new IssueInfo
                {
                    Id = i.Id,
                    StatusId = i.StatusId,
                    PriorityId = i.PriorityId,
                    TypeId = i.TypeId,
                    EmployeeId = i.AssigneeId!.Value
                })
                .ToListAsync(ct);

            return result;
        }

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

        public async Task<List<TimeChartPointInfo>> GetSpentTimeChartByEmployees(DateTime dateFrom, DateTime dateTo, string timeAxis, string granularity, IReadOnlyCollection<int> employeeIds, CancellationToken ct)
        {
            IQueryable<TimeEntryAxisProjection> query = BuildTimeAxisQuery(dateFrom, dateTo, timeAxis)
                .Where(x => employeeIds.Contains(x.EmployeeId));

            List<TimeChartAggregateProjection> rows = await BuildAggregatedEmployeeQuery(query, granularity)
                .OrderBy(x => x.EntityId)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ThenBy(x => x.Hour)
                .ThenBy(x => x.Minute)
                .ToListAsync(ct);

            return rows.Select(MapToPoint).ToList();
        }

        public async Task<List<TimeChartPointInfo>> GetSpentTimeChartByGroups(DateTime dateFrom, DateTime dateTo, string timeAxis, string granularity, IReadOnlyCollection<int> groupIds, CancellationToken ct)
        {
            IQueryable<TimeEntryAxisProjection> query = BuildTimeAxisQuery(dateFrom, dateTo, timeAxis);
            IQueryable<EmployeeGroup> groupQuery = employeeGroups.Query(asNoTracking: true)
                .Where(x => groupIds.Contains(x.GroupId));

            List<TimeChartAggregateProjection> rows = await BuildAggregatedGroupQuery(query, groupQuery, granularity)
                .OrderBy(x => x.EntityId)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ThenBy(x => x.Hour)
                .ThenBy(x => x.Minute)
                .ToListAsync(ct);

            return rows.Select(MapToPoint).ToList();
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
                query = query.Where(i => i.TypeId != null && i.TypeId != null && filters.TypeIds!.Contains(i.TypeId.Value));

            return query;
        }

        private IQueryable<TimeEntryAxisProjection> BuildTimeAxisQuery(DateTime dateFrom, DateTime dateTo, string timeAxis)
        {
            if (string.Equals(timeAxis, "createdAt", StringComparison.OrdinalIgnoreCase))
            {
                return timeEntries.Query(asNoTracking: true)
                    .Where(te => te.CreatedAt != null)
                    .Where(te => te.CreatedAt >= dateFrom && te.CreatedAt <= dateTo)
                    .Select(te => new TimeEntryAxisProjection
                    {
                        EmployeeId = te.EmployeeId,
                        TimeValue = te.CreatedAt!.Value,
                        SpentedTime = te.SpentTime
                    });
            }

            return timeEntries.Query(asNoTracking: true)
                .Where(te => te.LoggedAt >= dateFrom && te.LoggedAt <= dateTo)
                .Select(te => new TimeEntryAxisProjection
                {
                    EmployeeId = te.EmployeeId,
                    TimeValue = te.LoggedAt,
                    SpentedTime = te.SpentTime
                });
        }

        private static IQueryable<TimeChartAggregateProjection> BuildAggregatedEmployeeQuery(IQueryable<TimeEntryAxisProjection> query, string granularity)
        {
            if (string.Equals(granularity, "minute", StringComparison.OrdinalIgnoreCase))
            {
                return query
                    .GroupBy(x => new
                    {
                        EntityId = x.EmployeeId,
                        x.TimeValue.Year,
                        x.TimeValue.Month,
                        x.TimeValue.Day,
                        x.TimeValue.Hour,
                        x.TimeValue.Minute
                    })
                    .Select(g => new TimeChartAggregateProjection
                    {
                        EntityId = g.Key.EntityId,
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Day = g.Key.Day,
                        Hour = g.Key.Hour,
                        Minute = g.Key.Minute,
                        SpentedTime = g.Sum(x => x.SpentedTime)
                    });
            }

            if (string.Equals(granularity, "hour", StringComparison.OrdinalIgnoreCase))
            {
                return query
                    .GroupBy(x => new
                    {
                        EntityId = x.EmployeeId,
                        x.TimeValue.Year,
                        x.TimeValue.Month,
                        x.TimeValue.Day,
                        x.TimeValue.Hour
                    })
                    .Select(g => new TimeChartAggregateProjection
                    {
                        EntityId = g.Key.EntityId,
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Day = g.Key.Day,
                        Hour = g.Key.Hour,
                        Minute = 0,
                        SpentedTime = g.Sum(x => x.SpentedTime)
                    });
            }

            return query
                .GroupBy(x => new
                {
                    EntityId = x.EmployeeId,
                    x.TimeValue.Year,
                    x.TimeValue.Month,
                    x.TimeValue.Day
                })
                .Select(g => new TimeChartAggregateProjection
                {
                    EntityId = g.Key.EntityId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    Hour = 0,
                    Minute = 0,
                    SpentedTime = g.Sum(x => x.SpentedTime)
                });
        }

        private static IQueryable<TimeChartAggregateProjection> BuildAggregatedGroupQuery(IQueryable<TimeEntryAxisProjection> query, IQueryable<EmployeeGroup> groupQuery, string granularity)
        {
            if (string.Equals(granularity, "minute", StringComparison.OrdinalIgnoreCase))
            {
                return query
                    .Join(
                        groupQuery,
                        timeEntry => timeEntry.EmployeeId,
                        employeeGroup => employeeGroup.EmployeeId,
                        (timeEntry, employeeGroup) => new
                        {
                            EntityId = employeeGroup.GroupId,
                            timeEntry.TimeValue,
                            timeEntry.SpentedTime
                        })
                    .GroupBy(x => new
                    {
                        x.EntityId,
                        x.TimeValue.Year,
                        x.TimeValue.Month,
                        x.TimeValue.Day,
                        x.TimeValue.Hour,
                        x.TimeValue.Minute
                    })
                    .Select(g => new TimeChartAggregateProjection
                    {
                        EntityId = g.Key.EntityId,
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Day = g.Key.Day,
                        Hour = g.Key.Hour,
                        Minute = g.Key.Minute,
                        SpentedTime = g.Sum(x => x.SpentedTime)
                    });
            }

            if (string.Equals(granularity, "hour", StringComparison.OrdinalIgnoreCase))
            {
                return query
                    .Join(
                        groupQuery,
                        timeEntry => timeEntry.EmployeeId,
                        employeeGroup => employeeGroup.EmployeeId,
                        (timeEntry, employeeGroup) => new
                        {
                            EntityId = employeeGroup.GroupId,
                            timeEntry.TimeValue,
                            timeEntry.SpentedTime
                        })
                    .GroupBy(x => new
                    {
                        x.EntityId,
                        x.TimeValue.Year,
                        x.TimeValue.Month,
                        x.TimeValue.Day,
                        x.TimeValue.Hour
                    })
                    .Select(g => new TimeChartAggregateProjection
                    {
                        EntityId = g.Key.EntityId,
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Day = g.Key.Day,
                        Hour = g.Key.Hour,
                        Minute = 0,
                        SpentedTime = g.Sum(x => x.SpentedTime)
                    });
            }

            return query
                .Join(
                    groupQuery,
                    timeEntry => timeEntry.EmployeeId,
                    employeeGroup => employeeGroup.EmployeeId,
                    (timeEntry, employeeGroup) => new
                    {
                        EntityId = employeeGroup.GroupId,
                        timeEntry.TimeValue,
                        timeEntry.SpentedTime
                    })
                .GroupBy(x => new
                {
                    x.EntityId,
                    x.TimeValue.Year,
                    x.TimeValue.Month,
                    x.TimeValue.Day
                })
                .Select(g => new TimeChartAggregateProjection
                {
                    EntityId = g.Key.EntityId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    Hour = 0,
                    Minute = 0,
                    SpentedTime = g.Sum(x => x.SpentedTime)
                });
        }

        private static TimeChartPointInfo MapToPoint(TimeChartAggregateProjection row)
        {
            return new TimeChartPointInfo
            {
                EntityId = row.EntityId,
                BucketStart = new DateTime(row.Year, row.Month, row.Day, row.Hour ?? 0, row.Minute ?? 0, 0),
                SpentedTime = row.SpentedTime
            };
        }

        private class TimeEntryAxisProjection
        {
            public int EmployeeId { get; set; }

            public DateTime TimeValue { get; set; }

            public double SpentedTime { get; set; }
        }

        private class TimeChartAggregateProjection
        {
            public int EntityId { get; set; }

            public int Year { get; set; }

            public int Month { get; set; }

            public int Day { get; set; }

            public int? Hour { get; set; }

            public int? Minute { get; set; }

            public double SpentedTime { get; set; }
        }
    }
}