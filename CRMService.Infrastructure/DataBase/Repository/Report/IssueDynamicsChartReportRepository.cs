using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Application.Models.Report;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Infrastructure.DataBase.Models.Report;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Report
{
    public class IssueDynamicsChartReportRepository(IQueryRepository<Issue, MainContext> issues) : IIssueDynamicsChartReportRepository
    {
        public async Task<List<IssueDynamicsPointInfo>> GetCreatedIssuesAsync(DateTime dateFrom, DateTime dateTo, string granularity, CancellationToken ct)
        {
            IQueryable<Issue> query = issues.Query(asNoTracking: true)
                .Where(issue => issue.DeletedAt == null)
                .Where(issue => issue.CreatedAt >= dateFrom && issue.CreatedAt <= dateTo);

            List<IssueDynamicsAggregateProjection> rows = await BuildAggregateQuery(query.Select(issue => issue.CreatedAt), granularity)
                .OrderBy(row => row.Year)
                .ThenBy(row => row.Month)
                .ThenBy(row => row.Day)
                .ThenBy(row => row.Hour)
                .ToListAsync(ct);

            return rows.Select(MapToPoint).ToList();
        }

        public async Task<List<IssueDynamicsPointInfo>> GetCompletedIssuesAsync(DateTime dateFrom, DateTime dateTo, string granularity, CancellationToken ct)
        {
            IQueryable<Issue> query = issues.Query(asNoTracking: true)
                .Where(issue => issue.DeletedAt == null)
                .Where(issue => issue.CompletedAt != null)
                .Where(issue => issue.CompletedAt >= dateFrom && issue.CompletedAt <= dateTo);

            List<IssueDynamicsAggregateProjection> rows = await BuildAggregateQuery(query.Select(issue => issue.CompletedAt!.Value), granularity)
                .OrderBy(row => row.Year)
                .ThenBy(row => row.Month)
                .ThenBy(row => row.Day)
                .ThenBy(row => row.Hour)
                .ToListAsync(ct);

            return rows.Select(MapToPoint).ToList();
        }

        private static IQueryable<IssueDynamicsAggregateProjection> BuildAggregateQuery(IQueryable<DateTime> query, string granularity)
        {
            if (string.Equals(granularity, "hour", StringComparison.OrdinalIgnoreCase))
            {
                return query
                    .GroupBy(value => new
                    {
                        value.Year,
                        value.Month,
                        value.Day,
                        value.Hour
                    })
                    .Select(group => new IssueDynamicsAggregateProjection
                    {
                        Year = group.Key.Year,
                        Month = group.Key.Month,
                        Day = group.Key.Day,
                        Hour = group.Key.Hour,
                        Count = group.Count()
                    });
            }

            return query
                .GroupBy(value => new
                {
                    value.Year,
                    value.Month,
                    value.Day
                })
                .Select(group => new IssueDynamicsAggregateProjection
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Day = group.Key.Day,
                    Hour = 0,
                    Count = group.Count()
                });
        }

        private static IssueDynamicsPointInfo MapToPoint(IssueDynamicsAggregateProjection row)
        {
            return new IssueDynamicsPointInfo
            {
                BucketStart = new DateTime(row.Year, row.Month, row.Day, row.Hour, 0, 0),
                Count = row.Count
            };
        }
    }
}