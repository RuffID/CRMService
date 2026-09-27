using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.CrmEntities;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Abstractions.Service;

namespace CRMService.Application.Service.Report
{
    public class EmployeePerformanceReportService(IReportsUnitOfWork unitOfWork) : IEmployeePerformanceReportService
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
                employeeIds = (await unitOfWork.EmployeeGroup.GetByGroupIdsReadOnlyAsync(filters.GroupIds!, ct))
                    .Select(x => x.EmployeeId)
                    .ToList();
            }
            else
            {
                employeeIds = (await unitOfWork.Employee.GetItemsReadOnlyAsync(ct)).Select(e => e.Id).ToList();
            }

            List<Employee> employees = employeeIds.Count == 0
                ? new()
                : await unitOfWork.Employee.GetByIdsReadOnlyAsync(employeeIds, activeOnly: filters.ActiveOnly, ct: ct);

            employeeIds = employees.Select(employee => employee.Id).Distinct().ToList();
            Dictionary<int, Employee> employeeMap = employees.ToDictionary(e => e.Id, e => e);

            Dictionary<int, PlanSetting> planMap = new();
            string? planColor = null;

            if (employeeIds.Count > 0 && filters.PlanId.HasValue && filters.PlanId.Value != Guid.Empty)
            {
                Guid planId = filters.PlanId.Value;

                Plan? plan = await unitOfWork.Plan.GetItemByIdReadOnlyAsync(planId, ct);
                if (plan != null)
                {
                    planColor = plan.PlanColor;

                    List<PlanSetting> planSettings = await unitOfWork.PlanSetting.GetByPlanAndEmployeesReadOnlyAsync(planId, employeeIds, ct);

                    planMap = planSettings.ToDictionary(x => x.EmployeeId, x => x);
                }
            }

            List<ReportInfo> result = new(employeeIds.Count + (filters.IncludeUnassigned ? 1 : 0));

            if (employeeIds.Count > 0)
            {
                ReportRequest effectiveFilters = new()
                {
                    EmployeeIds = employeeIds,
                    StatusIds = filters.StatusIds,
                    PriorityIds = filters.PriorityIds,
                    TypeIds = filters.TypeIds,
                    GroupIds = null,
                    HideWithoutSolved = filters.HideWithoutSolved,
                    HideWithoutCurrent = filters.HideWithoutCurrent,
                    HideWithoutTime = filters.HideWithoutTime,
                    ActiveOnly = filters.ActiveOnly
                };

                List<SolvedIssuesCountInfo> openCounts = await unitOfWork.EmployeePerformanceReport.GetOpenIssuesCountByEmployees(effectiveFilters, ct);
                List<SolvedIssuesCountInfo> solvedCounts = await unitOfWork.EmployeePerformanceReport.GetSolvedIssuesCountByEmployees(dateFrom, dateTo, effectiveFilters, ct);
                List<SpentedTimeInfo> spentTimes = await unitOfWork.EmployeePerformanceReport.GetSpentedTimeByEmployee(dateFrom, dateTo, effectiveFilters, ct);

                Dictionary<int, int> currentByEmployee = openCounts.ToDictionary(x => x.EmployeeId, x => x.Count);
                Dictionary<int, int> solvedByEmployee = solvedCounts.ToDictionary(x => x.EmployeeId, x => x.Count);
                Dictionary<int, double> spentByEmployee = spentTimes.ToDictionary(x => x.EmployeeId, x => x.SpentedTime);

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
            }

            await AddUnassignedGroupRowAsync(result, filters, ct);

            return result;
        }

        private async Task AddUnassignedGroupRowAsync(List<ReportInfo> result, ReportRequest filters, CancellationToken ct)
        {
            if (!filters.IncludeUnassigned || filters.UnassignedGroupId is not > 0)
                return;

            int groupId = filters.UnassignedGroupId.Value;
            List<Group> groups = await unitOfWork.Group.GetByIdsReadOnlyAsync([groupId], ct);
            Group? group = groups.SingleOrDefault();
            if (group == null)
                return;

            ReportRequest issueFilters = new()
            {
                StatusIds = filters.StatusIds,
                PriorityIds = filters.PriorityIds,
                TypeIds = filters.TypeIds
            };

            int current = await unitOfWork.EmployeePerformanceReport.GetOpenUnassignedIssuesCount(groupId, issueFilters, ct);
            if (filters.HideWithoutCurrent && current == 0)
                return;

            result.Add(new ReportInfo
            {
                ResponsibleGroupId = groupId,
                IsUnassigned = true,
                DisplayName = $"{group.Name} — без ответственного",
                CurrentIssuesCount = current
            });
        }
    }
}
