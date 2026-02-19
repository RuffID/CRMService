using CRMService.Abstractions.Database.Repository;
using CRMService.Abstractions.Service;
using CRMService.Models.CrmEntities;
using CRMService.Models.Dto.CrmEntities;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.Responses.Results;
using CRMService.Service.OkdeskEntity;

namespace CRMService.Service.CrmServices
{
    public class PlanSettingsService(IUnitOfWork unitOfWork, EmployeeService employeeService) : IPlanSettingsService
    {
        public async Task<ServiceResult<List<EmployeePlanRowDto>>> GetEmployeePlanRows(CancellationToken ct = default)
        {
            ServiceResult<List<EmployeeDto>> employeesResult = await employeeService.GetEmployees(groupIds: null, ct: ct);

            if (!employeesResult.Success)
                return ServiceResult<List<EmployeePlanRowDto>>.Fail(employeesResult.Error!.StatusCode, employeesResult.Error.Message);

            List<EmployeeDto> employees = employeesResult.Data ?? new();

            List<int> employeeIds = employees.Select(x => x.Id).ToList();

            List<PlanSetting> settings = await unitOfWork.PlanSetting
                .GetItemsByPredicateAsync(x => employeeIds.Contains(x.EmployeeId), asNoTracking: true, ct: ct);

            Dictionary<int, PlanSetting> map = settings.ToDictionary(x => x.EmployeeId, x => x);

            List<EmployeePlanRowDto> rows = employees
                .Where(x => x.Active)
                .Select(e =>
                {
                    map.TryGetValue(e.Id, out PlanSetting? ps);

                    return new EmployeePlanRowDto
                    {
                        EmployeeId = e.Id,
                        FullName = BuildFullName(e.LastName, e.FirstName, e.Patronymic),
                        MonthPlan = ps?.MonthPlan,
                        DayPlan = ps?.DayPlan,
                        Groups = e.Groups
                    };
                })
                .OrderBy(x => x.FullName)
                .ToList();

            return ServiceResult<List<EmployeePlanRowDto>>.Ok(rows);
        }

        public async Task<ServiceResult<bool>> SavePlanSettings(List<PlanSettingDto> items, CancellationToken ct = default)
        {
            if (items == null)
                return ServiceResult<bool>.Fail(400, "Items is required.");

            foreach (PlanSettingDto it in items)
            {
                if (it.MonthPlan.HasValue && it.MonthPlan.Value < 0)
                    return ServiceResult<bool>.Fail(400, "MonthPlan must be >= 0.");

                if (it.DayPlan.HasValue && it.DayPlan.Value < 0)
                    return ServiceResult<bool>.Fail(400, "DayPlan must be >= 0.");
            }

            List<int> ids = items.Select(x => x.EmployeeId).Distinct().ToList();

            List<PlanSetting> existing = await unitOfWork.PlanSetting.GetItemsByPredicateAsync(x => ids.Contains(x.EmployeeId), ct: ct);

            Dictionary<int, PlanSetting> map = existing.ToDictionary(x => x.EmployeeId, x => x);

            foreach (PlanSettingDto dto in items)
            {
                bool isEmpty = !dto.MonthPlan.HasValue && !dto.DayPlan.HasValue;

                if (map.TryGetValue(dto.EmployeeId, out PlanSetting? entity))
                {
                    if (isEmpty)
                    {
                        unitOfWork.PlanSetting.Delete(entity);
                        continue;
                    }

                    entity.MonthPlan = dto.MonthPlan;
                    entity.DayPlan = dto.DayPlan;
                }
                else
                {
                    if (isEmpty)
                        continue;

                    PlanSetting ps = new ()
                    {
                        EmployeeId = dto.EmployeeId,
                        MonthPlan = dto.MonthPlan,
                        DayPlan = dto.DayPlan,
                    };

                    unitOfWork.PlanSetting.Create(ps);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<List<PlanColorSchemeDto>>> GetPlanColorSchemes(CancellationToken ct = default)
        {
            List<PlanColorScheme> rules = await unitOfWork.PlanColor
                .GetItemsByPredicateAsync(_ => true, asNoTracking: true, ct: ct);

            List<PlanColorSchemeDto> dto = rules
                .OrderBy(x => x.FromPercent)
                .ThenBy(x => x.ToPercent)
                .Select(x => new PlanColorSchemeDto
                {
                    Id = x.Id,
                    FromPercent = x.FromPercent,
                    ToPercent = x.ToPercent,
                    Color = x.Color
                })
                .ToList();

            return ServiceResult<List<PlanColorSchemeDto>>.Ok(dto);
        }

        public async Task<ServiceResult<bool>> SavePlanColorSchemes(List<PlanColorSchemeDto> items, CancellationToken ct = default)
        {
            if (items == null)
                return ServiceResult<bool>.Fail(400, "Items is required.");

            foreach (PlanColorSchemeDto x in items)
            {
                if (x.FromPercent < 0 || x.ToPercent < 0)
                    return ServiceResult<bool>.Fail(400, "Percent must be >= 0.");

                if (x.ToPercent <= x.FromPercent)
                    return ServiceResult<bool>.Fail(400, "ToPercent must be greater than FromPercent.");

                x.Color = string.IsNullOrWhiteSpace(x.Color) ? "#198754" : x.Color.Trim();
            }

            List<PlanColorScheme> existing = await unitOfWork.PlanColor.GetItemsByPredicateAsync(_ => true, ct: ct);

            Dictionary<Guid, PlanColorScheme> map = existing.ToDictionary(x => x.Id, x => x);

            HashSet<Guid?> keepIds = items.Where(x => x.Id.HasValue).Select(x => x.Id).ToHashSet();

            foreach (PlanColorScheme e in existing)
            {
                if (!keepIds.Contains(e.Id))
                    unitOfWork.PlanColor.Delete(e);
            }

            foreach (PlanColorSchemeDto dto in items)
            {
                if (dto.Id.HasValue && map.TryGetValue(dto.Id.Value, out PlanColorScheme? entity))
                {
                    entity.FromPercent = dto.FromPercent;
                    entity.ToPercent = dto.ToPercent;
                    entity.Color = dto.Color;
                }
                else
                {
                    PlanColorScheme rule = new ()
                    {
                        FromPercent = dto.FromPercent,
                        ToPercent = dto.ToPercent,
                        Color = dto.Color
                    };

                    unitOfWork.PlanColor.Create(rule);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        private static string BuildFullName(string? lastName, string? firstName, string? patronymic)
        {
            string ln = lastName ?? "";
            string fn = firstName ?? "";
            string pn = patronymic ?? "";
            return string.Join(" ", new[] { ln, fn, pn }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        }
    }
}
