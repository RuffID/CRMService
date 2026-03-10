using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;

namespace CRMService.Application.Service.Report
{
    public class IssueDynamicsChartService(IUnitOfWork unitOfWork) : IIssueDynamicsChartService
    {
        public async Task<IssueDynamicsChartDto> GetIssueDynamicsChartAsync(IssueDynamicsChartRequest request, CancellationToken ct)
        {
            string granularity = string.Equals(request.Granularity, "hour", StringComparison.OrdinalIgnoreCase) ? "hour" : "day";
            List<DateTime> buckets = BuildBuckets(request.DateFrom, request.DateTo, granularity);

            List<IssueDynamicsPointInfo> createdPoints = await unitOfWork.IssueDynamicsChartReport.GetCreatedIssuesAsync(
                request.DateFrom,
                request.DateTo,
                granularity,
                ct);

            List<IssueDynamicsPointInfo> completedPoints = await unitOfWork.IssueDynamicsChartReport.GetCompletedIssuesAsync(
                request.DateFrom,
                request.DateTo,
                granularity,
                ct);

            return new IssueDynamicsChartDto
            {
                Granularity = granularity,
                Buckets = buckets,
                CreatedValues = BuildValues(buckets, createdPoints),
                CompletedValues = BuildValues(buckets, completedPoints)
            };
        }

        private static List<int> BuildValues(List<DateTime> buckets, List<IssueDynamicsPointInfo> points)
        {
            Dictionary<DateTime, int> pointMap = points
                .GroupBy(x => x.BucketStart)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            List<int> values = new(buckets.Count);

            foreach (DateTime bucket in buckets)
            {
                pointMap.TryGetValue(bucket, out int value);
                values.Add(value);
            }

            return values;
        }

        private static List<DateTime> BuildBuckets(DateTime dateFrom, DateTime dateTo, string granularity)
        {
            DateTime current = NormalizeBucketStart(dateFrom, granularity);
            DateTime last = NormalizeBucketStart(dateTo, granularity);
            List<DateTime> buckets = new();

            while (current <= last)
            {
                buckets.Add(current);
                current = granularity == "hour"
                    ? current.AddHours(1)
                    : current.AddDays(1);
            }

            return buckets;
        }

        private static DateTime NormalizeBucketStart(DateTime value, string granularity)
        {
            return granularity == "hour"
                ? new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0)
                : new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
        }
    }
}