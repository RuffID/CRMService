using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Application.Models.Report;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Infrastructure.DataBase.Models.Report;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Report
{
    public class SpentTimeChartReportRepository(
        IQueryRepository<TimeEntry, MainContext> timeEntries,
        IQueryRepository<EmployeeGroup, MainContext> employeeGroups) : ISpentTimeChartReportRepository
    {
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
    }
}
