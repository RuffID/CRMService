using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.CrmEntities;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.Report
{
    public class ReportService(IUnitOfWork unitOfWork) : IReportService
    {
        public async Task<List<ReportInfo>> GetFullReportOnEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest filters, CancellationToken ct)
        {
            List<int> employeeIds;

            if (filters.HasEmployees)
            {
                employeeIds = filters.EmployeeIds!.Distinct().ToList();
            }
            else if (filters.HasGroups)
            {
                employeeIds = (await unitOfWork.EmployeeGroup.GetItemsByPredicateAsync(
                        eg => filters.GroupIds!.Contains(eg.GroupId),
                        asNoTracking: true,
                        ct: ct)).Select(x => x.EmployeeId).ToList();
            }
            else
            {
                employeeIds = (await unitOfWork.Employee.GetItemsByPredicateAsync(asNoTracking: true, ct: ct)).Select(e => e.Id).ToList();
            }

            if (employeeIds.Count == 0)
                return new();

            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(e => employeeIds.Contains(e.Id), asNoTracking: true, ct: ct);

            Dictionary<int, Employee> employeeMap = employees.ToDictionary(e => e.Id, e => e);

            Dictionary<int, PlanSetting> planMap = new();
            string? planColor = null;

            if (filters.PlanId.HasValue && filters.PlanId.Value != Guid.Empty)
            {
                Guid planId = filters.PlanId.Value;

                Plan? plan = await unitOfWork.Plan.GetItemByIdAsync(planId, asNoTracking: true, ct: ct);
                if (plan != null)
                {
                    planColor = plan.PlanColor;

                    List<PlanSetting> planSettings = await unitOfWork.PlanSetting.GetItemsByPredicateAsync(
                        x => x.PlanId == planId && employeeIds.Contains(x.EmployeeId),
                        asNoTracking: true,
                        ct: ct);

                    planMap = planSettings.ToDictionary(x => x.EmployeeId, x => x);
                }
            }

            ReportRequest effectiveFilters = new()
            {
                EmployeeIds = employeeIds,
                StatusIds = filters.StatusIds,
                PriorityIds = filters.PriorityIds,
                TypeIds = filters.TypeIds,
                GroupIds = null,
                HideWithoutSolved = filters.HideWithoutSolved,
                HideWithoutCurrent = filters.HideWithoutCurrent,
                HideWithoutTime = filters.HideWithoutTime
            };

            List<SolvedIssuesCountInfo> openCounts = await unitOfWork.Report.GetOpenIssuesCountByEmployees(effectiveFilters, ct);
            List<SolvedIssuesCountInfo> solvedCounts = await unitOfWork.Report.GetSolvedIssuesCountByEmployees(dateFrom, dateTo, effectiveFilters, ct);
            List<SpentedTimeInfo> spentTimes = await unitOfWork.Report.GetSpentedTimeByEmployee(dateFrom, dateTo, effectiveFilters, ct);

            Dictionary<int, int> currentByEmployee = openCounts.ToDictionary(x => x.EmployeeId, x => x.Count);
            Dictionary<int, int> solvedByEmployee = solvedCounts.ToDictionary(x => x.EmployeeId, x => x.Count);
            Dictionary<int, double> spentByEmployee = spentTimes.ToDictionary(x => x.EmployeeId, x => x.SpentedTime);

            List<ReportInfo> result = new(employeeIds.Count);

            foreach (int employeeId in employeeIds)
            {
                currentByEmployee.TryGetValue(employeeId, out int current);
                solvedByEmployee.TryGetValue(employeeId, out int solved);
                spentByEmployee.TryGetValue(employeeId, out double spent);
                employeeMap.TryGetValue(employeeId, out Employee? employee);
                planMap.TryGetValue(employeeId, out PlanSetting? ps);

                if (effectiveFilters.HideWithoutSolved && solved == 0)
                    continue;

                if (effectiveFilters.HideWithoutCurrent && current == 0)
                    continue;

                if (effectiveFilters.HideWithoutTime && spent == 0)
                    continue;

                if (!effectiveFilters.HideWithoutSolved && !effectiveFilters.HideWithoutCurrent && !effectiveFilters.HideWithoutTime)
                {
                    if (current == 0 && solved == 0 && spent == 0)
                        continue;
                }

                result.Add(new ReportInfo
                {
                    EmployeeId = employeeId,
                    FirstName = employee?.FirstName,
                    LastName = employee?.LastName,
                    Patronymic = employee?.Patronymic,
                    CurrentIssuesCount = current,
                    SolvedIssues = solved,
                    SpentedTime = spent,
                    PlanId = filters.PlanId,
                    PlanValue = ps?.PlanValue,
                    PlanColor = planColor
                });
            }

            return result;
        }

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

            List<TimeChartPointInfo> points = await unitOfWork.Report.GetSpentTimeChartByEmployees(
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

            List<TimeChartPointInfo> points = await unitOfWork.Report.GetSpentTimeChartByGroups(
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
