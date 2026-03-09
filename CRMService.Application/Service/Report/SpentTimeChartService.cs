using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.Report
{
    public class SpentTimeChartService(IUnitOfWork unitOfWork) : ISpentTimeChartService
    {
        public async Task<TimeChartDto> GetSpentTimeChart(TimeChartRequest request, CancellationToken ct)
        {
            string scope = string.Equals(request.Scope, "group", StringComparison.OrdinalIgnoreCase) ? "group" : "employee";
            string timeAxis = string.Equals(request.TimeAxis, "createdAt", StringComparison.OrdinalIgnoreCase) ? "createdAt" : "loggedAt";
            string granularity = string.Equals(request.Granularity, "hour", StringComparison.OrdinalIgnoreCase) ? "hour" : "day";
            List<DateTime> buckets = BuildBuckets(request.DateFrom, request.DateTo, granularity);

            if (scope == "employee")
                return await BuildEmployeeChart(request, timeAxis, granularity, buckets, ct);

            return await BuildGroupChart(request, timeAxis, granularity, buckets, ct);
        }

        private async Task<TimeChartDto> BuildEmployeeChart(TimeChartRequest request, string timeAxis, string granularity, List<DateTime> buckets, CancellationToken ct)
        {
            List<int> employeeIds = await ResolveEmployeeIds(request, ct);
            if (employeeIds.Count == 0)
            {
                return new TimeChartDto
                {
                    Scope = "employee",
                    TimeAxis = timeAxis,
                    Granularity = granularity,
                    Buckets = buckets
                };
            }

            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(
                e => employeeIds.Contains(e.Id) && e.Active,
                asNoTracking: true,
                ct: ct);

            Dictionary<int, string> employeeNames = employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ThenBy(e => e.Patronymic)
                .ToDictionary(e => e.Id, BuildEmployeeName);

            List<TimeChartPointInfo> points = await unitOfWork.SpentTimeChartReport.GetSpentTimeChartByEmployees(
                request.DateFrom,
                request.DateTo,
                timeAxis,
                granularity,
                employeeIds,
                ct);

            return new TimeChartDto
            {
                Scope = "employee",
                TimeAxis = timeAxis,
                Granularity = granularity,
                Buckets = buckets,
                Series = BuildSeries(employeeIds, employeeNames, points, buckets)
            };
        }

        private async Task<TimeChartDto> BuildGroupChart(TimeChartRequest request, string timeAxis, string granularity, List<DateTime> buckets, CancellationToken ct)
        {
            List<int> groupIds = await ResolveGroupIds(request, ct);
            if (groupIds.Count == 0)
            {
                return new TimeChartDto
                {
                    Scope = "group",
                    TimeAxis = timeAxis,
                    Granularity = granularity,
                    Buckets = buckets
                };
            }

            List<Group> groups = await unitOfWork.Group.GetItemsByPredicateAsync(
                g => groupIds.Contains(g.Id),
                asNoTracking: true,
                ct: ct);

            Dictionary<int, string> groupNames = groups
                .OrderBy(g => g.Name)
                .ToDictionary(g => g.Id, g => string.IsNullOrWhiteSpace(g.Name) ? $"Группа {g.Id}" : g.Name!);

            List<TimeChartPointInfo> points = await unitOfWork.SpentTimeChartReport.GetSpentTimeChartByGroups(
                request.DateFrom,
                request.DateTo,
                timeAxis,
                granularity,
                groupIds,
                ct);

            return new TimeChartDto
            {
                Scope = "group",
                TimeAxis = timeAxis,
                Granularity = granularity,
                Buckets = buckets,
                Series = BuildSeries(groupIds, groupNames, points, buckets)
            };
        }

        private async Task<List<int>> ResolveEmployeeIds(TimeChartRequest request, CancellationToken ct)
        {
            if (request.HasEmployees)
                return request.EmployeeIds!.Distinct().ToList();

            if (request.HasGroups)
            {
                List<int> groupIds = request.GroupIds!.Distinct().ToList();

                List<EmployeeGroup> connections = await unitOfWork.EmployeeGroup.GetItemsByPredicateAsync(
                        eg => groupIds.Contains(eg.GroupId),
                        asNoTracking: true,
                        ct: ct);

                return connections.Select(x => x.EmployeeId).Distinct().ToList();
            }

            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(
                    e => e.Active,
                    asNoTracking: true,
                    ct: ct);

            return employees.Select(e => e.Id).ToList();
        }

        private async Task<List<int>> ResolveGroupIds(TimeChartRequest request, CancellationToken ct)
        {
            if (request.HasGroups)
                return request.GroupIds!.Distinct().ToList();

            return (await unitOfWork.Group.GetItemsByPredicateAsync(asNoTracking: true, ct: ct)).Select(g => g.Id).ToList();
        }

        private static List<TimeChartSeriesDto> BuildSeries(List<int> entityIds, Dictionary<int, string> entityNames, List<TimeChartPointInfo> points, List<DateTime> buckets)
        {
            Dictionary<(int EntityId, DateTime BucketStart), double> pointMap = points
                .GroupBy(x => (x.EntityId, x.BucketStart))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.SpentedTime));

            List<TimeChartSeriesDto> series = new(entityIds.Count);

            foreach (int entityId in entityIds.OrderBy(x => entityNames.TryGetValue(x, out string? name) ? name : x.ToString()))
            {
                string entityName = entityNames.TryGetValue(entityId, out string? name) && !string.IsNullOrWhiteSpace(name)
                    ? name
                    : entityId.ToString();

                List<double> values = new(buckets.Count);

                foreach (DateTime bucket in buckets)
                {
                    pointMap.TryGetValue((entityId, bucket), out double value);
                    values.Add(value);
                }

                series.Add(new TimeChartSeriesDto
                {
                    Id = entityId.ToString(),
                    Name = entityName,
                    Values = values
                });
            }

            return series;
        }

        private static List<DateTime> BuildBuckets(DateTime dateFrom, DateTime dateTo, string granularity)
        {
            DateTime current = NormalizeBucketStart(dateFrom, granularity);
            DateTime last = NormalizeBucketStart(dateTo, granularity);
            List<DateTime> buckets = new();

            while (current <= last)
            {
                buckets.Add(current);
                if (granularity == "hour")
                    current = current.AddHours(1);
                else
                    current = current.AddDays(1);
            }

            return buckets;
        }

        private static DateTime NormalizeBucketStart(DateTime value, string granularity)
        {
            return granularity switch
            {
                "hour" => new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0),
                _ => new DateTime(value.Year, value.Month, value.Day, 0, 0, 0)
            };
        }

        private static string BuildEmployeeName(Employee employee)
        {
            string fullName = string.Join(
                " ",
                new[]
                {
                    employee.LastName?.Trim() ?? string.Empty,
                    employee.FirstName?.Trim() ?? string.Empty,
                    employee.Patronymic?.Trim() ?? string.Empty
                }.Where(x => !string.IsNullOrWhiteSpace(x)));

            return string.IsNullOrWhiteSpace(fullName) ? employee.Id.ToString() : fullName;
        }
    }
}