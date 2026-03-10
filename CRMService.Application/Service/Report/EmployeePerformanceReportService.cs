using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.CrmEntities;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.Report
{
    public class EmployeePerformanceReportService(IUnitOfWork unitOfWork) : IEmployeePerformanceReportService
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

            List<SolvedIssuesCountInfo> openCounts = await unitOfWork.EmployeePerformanceReport.GetOpenIssuesCountByEmployees(effectiveFilters, ct);
            List<SolvedIssuesCountInfo> solvedCounts = await unitOfWork.EmployeePerformanceReport.GetSolvedIssuesCountByEmployees(dateFrom, dateTo, effectiveFilters, ct);
            List<SpentedTimeInfo> spentTimes = await unitOfWork.EmployeePerformanceReport.GetSpentedTimeByEmployee(dateFrom, dateTo, effectiveFilters, ct);

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
                planMap.TryGetValue(employeeId, out PlanSetting? planSetting);

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
                    PlanValue = planSetting?.PlanValue,
                    PlanColor = planColor
                });
            }

            return result;
        }
    }
}