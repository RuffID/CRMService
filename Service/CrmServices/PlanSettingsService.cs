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
        private const int DefaultPlanSwitchSeconds = 10;

        public async Task<ServiceResult<List<PlanDto>>> GetPlans(CancellationToken ct = default)
        {
            List<Plan> plans = await unitOfWork.Plan.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            List<PlanDto> dto = plans.OrderBy(x => x.Name).Select(x => new PlanDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    PlanColor = x.PlanColor
                })
                .ToList();

            return ServiceResult<List<PlanDto>>.Ok(dto);
        }

        public async Task<ServiceResult<bool>> SavePlans(List<PlanDto> items, CancellationToken ct = default)
        {
            if (items == null)
                return ServiceResult<bool>.Fail(400, "Items is required.");

            List<PlanDto> normalizedItems = new (items.Count);

            foreach (PlanDto item in items)
            {
                string name = (item.Name ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name))
                    return ServiceResult<bool>.Fail(400, "Plan name is required.");

                if (name.Length > 200)
                    return ServiceResult<bool>.Fail(400, "Plan name length must be <= 200.");

                string? normalizedColor = NormalizePlanColor(item.PlanColor);
                if (item.PlanColor != null && normalizedColor == null)
                    return ServiceResult<bool>.Fail(400, "Plan color must be in format #RRGGBB.");

                normalizedItems.Add(new PlanDto
                {
                    Id = item.Id,
                    Name = name,
                    PlanColor = normalizedColor
                });
            }

            List<string> duplicateNames = normalizedItems
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateNames.Count > 0)
                return ServiceResult<bool>.Fail(400, "Plan names must be unique.");

            List<Plan> existing = await unitOfWork.Plan.GetItemsByPredicateAsync(ct: ct);
            Dictionary<Guid, Plan> existingMap = existing.ToDictionary(x => x.Id, x => x);
            HashSet<Guid> keepIds = normalizedItems
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id!.Value)
                .ToHashSet();

            foreach (Plan oldPlan in existing)
            {
                if (!keepIds.Contains(oldPlan.Id))
                    unitOfWork.Plan.Delete(oldPlan);
            }

            foreach (PlanDto item in normalizedItems)
            {
                if (item.Id.HasValue)
                {
                    Guid planId = item.Id.Value;
                    if (!existingMap.TryGetValue(planId, out Plan? plan))
                        return ServiceResult<bool>.Fail(400, $"Plan with id '{planId}' does not exist.");

                    plan.Name = item.Name;
                    plan.PlanColor = item.PlanColor;
                    continue;
                }

                Plan newPlan = new ()
                {
                    Name = item.Name,
                    PlanColor = item.PlanColor
                };

                unitOfWork.Plan.Create(newPlan);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<GeneralSettingsDto>> GetGeneralSettings(CancellationToken ct = default)
        {
            GeneralSettings settings = await EnsureGeneralSettings(ct);

            GeneralSettingsDto dto = new ()
            {
                Id = settings.Id,
                PlanSwitchSeconds = settings.PlanSwitchSeconds
            };

            return ServiceResult<GeneralSettingsDto>.Ok(dto);
        }

        public async Task<ServiceResult<bool>> SaveGeneralSettings(GeneralSettingsDto item, CancellationToken ct = default)
        {
            if (item.PlanSwitchSeconds < DefaultPlanSwitchSeconds)
                return ServiceResult<bool>.Fail(400, "PlanSwitchSeconds must be >= 10.");

            GeneralSettings settings = await EnsureGeneralSettings(ct);
            settings.PlanSwitchSeconds = item.PlanSwitchSeconds;

            await unitOfWork.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<List<EmployeePlanRowDto>>> GetEmployeePlanRows(Guid planId, CancellationToken ct = default)
        {
            if (planId == Guid.Empty)
                return ServiceResult<List<EmployeePlanRowDto>>.Fail(400, "PlanId is required.");

            Plan? plan = await unitOfWork.Plan.GetItemByIdAsync(planId, asNoTracking: true, ct: ct);
            if (plan == null)
                return ServiceResult<List<EmployeePlanRowDto>>.Fail(404, "Plan not found.");

            ServiceResult<List<EmployeeDto>> employeesResult = await employeeService.GetEmployees(groupIds: null, ct: ct);
            if (!employeesResult.Success)
                return ServiceResult<List<EmployeePlanRowDto>>.Fail(employeesResult.Error!.StatusCode, employeesResult.Error.Message);

            List<EmployeeDto> employees = employeesResult.Data ?? new ();
            List<int> employeeIds = employees.Select(x => x.Id).ToList();

            List<PlanSetting> settings = await unitOfWork.PlanSetting
                .GetItemsByPredicateAsync(
                    x => x.PlanId == planId && employeeIds.Contains(x.EmployeeId),
                    asNoTracking: true,
                    ct: ct);

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
                        PlanValue = ps?.PlanValue,
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

            HashSet<Guid> planIds = new HashSet<Guid>();
            HashSet<int> employeeIds = new HashSet<int>();

            foreach (PlanSettingDto item in items)
            {
                if (item.PlanId == Guid.Empty)
                    return ServiceResult<bool>.Fail(400, "PlanId is required.");

                if (item.EmployeeId <= 0)
                    return ServiceResult<bool>.Fail(400, "EmployeeId must be > 0.");

                if (item.PlanValue.HasValue && item.PlanValue.Value < 0)
                    return ServiceResult<bool>.Fail(400, "PlanValue must be >= 0.");

                planIds.Add(item.PlanId);
                employeeIds.Add(item.EmployeeId);
            }

            List<Plan> plans = await unitOfWork.Plan.GetItemsByPredicateAsync(x => planIds.Contains(x.Id), asNoTracking: true, ct: ct);

            if (plans.Count != planIds.Count)
                return ServiceResult<bool>.Fail(400, "One or more plans do not exist.");

            List<PlanSetting> existing = await unitOfWork.PlanSetting.GetItemsByPredicateAsync(
                x => planIds.Contains(x.PlanId) && employeeIds.Contains(x.EmployeeId),
                ct: ct);

            Dictionary<string, PlanSetting> map = existing.ToDictionary(
                x => ComposePlanSettingKey(x.PlanId, x.EmployeeId),
                x => x);

            foreach (PlanSettingDto dto in items)
            {
                string key = ComposePlanSettingKey(dto.PlanId, dto.EmployeeId);
                bool isEmpty = !dto.PlanValue.HasValue;

                if (map.TryGetValue(key, out PlanSetting? entity))
                {
                    if (isEmpty)
                    {
                        unitOfWork.PlanSetting.Delete(entity);
                        continue;
                    }

                    entity.PlanValue = dto.PlanValue;
                }
                else
                {
                    if (isEmpty)
                        continue;

                    PlanSetting planSetting = new ()
                    {
                        PlanId = dto.PlanId,
                        EmployeeId = dto.EmployeeId,
                        PlanValue = dto.PlanValue
                    };

                    unitOfWork.PlanSetting.Create(planSetting);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<List<PlanColorSchemeDto>>> GetPlanColorSchemes(Guid planId, CancellationToken ct = default)
        {
            if (planId == Guid.Empty)
                return ServiceResult<List<PlanColorSchemeDto>>.Fail(400, "PlanId is required.");

            List<PlanColorScheme> rules = await unitOfWork.PlanColor
                .GetItemsByPredicateAsync(x => x.PlanId == planId, asNoTracking: true, ct: ct);

            List<PlanColorSchemeDto> dto = rules
                .OrderBy(x => x.FromPercent)
                .ThenBy(x => x.ToPercent ?? int.MaxValue)
                .Select(x => new PlanColorSchemeDto
                {
                    Id = x.Id,
                    PlanId = x.PlanId,
                    FromPercent = x.FromPercent,
                    ToPercent = x.ToPercent,
                    Color = x.Color
                })
                .ToList();

            return ServiceResult<List<PlanColorSchemeDto>>.Ok(dto);
        }

        public async Task<ServiceResult<bool>> SavePlanColorSchemes(Guid planId, List<PlanColorSchemeDto> items, CancellationToken ct = default)
        {
            if (items == null)
                return ServiceResult<bool>.Fail(400, "Items is required.");

            if (planId == Guid.Empty)
                return ServiceResult<bool>.Fail(400, "PlanId is required.");

            foreach (PlanColorSchemeDto item in items)
            {
                if (item.PlanId != planId)
                    return ServiceResult<bool>.Fail(400, "All rules must reference the same PlanId.");

                if (item.FromPercent < 0)
                    return ServiceResult<bool>.Fail(400, "FromPercent must be >= 0.");

                if (item.ToPercent.HasValue && item.ToPercent.Value < item.FromPercent)
                    return ServiceResult<bool>.Fail(400, "ToPercent must be >= FromPercent.");

                string normalizedColor = NormalizeRequiredHexColor(item.Color);
                if (string.IsNullOrEmpty(normalizedColor))
                    return ServiceResult<bool>.Fail(400, "Color must be in format #RRGGBB.");

                item.Color = normalizedColor;
            }

            if (!ValidateRanges(items))
                return ServiceResult<bool>.Fail(400, "Color ranges must not overlap.");

            Plan? plan = await unitOfWork.Plan.GetItemByIdAsync(planId, asNoTracking: true, ct: ct);
            if (plan == null)
                return ServiceResult<bool>.Fail(404, "Plan not found.");

            List<PlanColorScheme> existing = await unitOfWork.PlanColor.GetItemsByPredicateAsync(x => x.PlanId == planId, ct: ct);
            Dictionary<Guid, PlanColorScheme> map = existing.ToDictionary(x => x.Id, x => x);
            HashSet<Guid?> keepIds = items.Where(x => x.Id.HasValue).Select(x => x.Id).ToHashSet();

            foreach (PlanColorScheme item in existing)
            {
                if (!keepIds.Contains(item.Id))
                    unitOfWork.PlanColor.Delete(item);
            }

            foreach (PlanColorSchemeDto dto in items)
            {
                if (dto.Id.HasValue && map.TryGetValue(dto.Id.Value, out PlanColorScheme? entity))
                {
                    entity.PlanId = dto.PlanId;
                    entity.FromPercent = dto.FromPercent;
                    entity.ToPercent = dto.ToPercent;
                    entity.Color = dto.Color;
                }
                else
                {
                    PlanColorScheme rule = new ()
                    {
                        PlanId = dto.PlanId,
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

        private async Task<GeneralSettings> EnsureGeneralSettings(CancellationToken ct)
        {
            List<GeneralSettings> settings = await unitOfWork.GeneralSettings.GetItemsByPredicateAsync(ct: ct);
            GeneralSettings? existing = settings.FirstOrDefault();

            if (existing != null)
                return existing;

            GeneralSettings created = new ()
            {
                PlanSwitchSeconds = DefaultPlanSwitchSeconds
            };

            unitOfWork.GeneralSettings.Create(created);
            await unitOfWork.SaveChangesAsync(ct);

            return created;
        }

        private static bool ValidateRanges(List<PlanColorSchemeDto> items)
        {
            List<(int From, int To)> ranges = items
                .Select(x => (x.FromPercent, x.ToPercent ?? int.MaxValue))
                .OrderBy(x => x.Item1)
                .ThenBy(x => x.Item2)
                .ToList();

            for (int index = 0; index < ranges.Count - 1; index++)
            {
                (int FromCurrent, int ToCurrent) = ranges[index];
                (int FromNext, int ToNext) = ranges[index + 1];

                if (FromNext <= ToCurrent)
                    return false;
            }

            return true;
        }

        private static string ComposePlanSettingKey(Guid planId, int employeeId)
        {
            return $"{planId:N}:{employeeId}";
        }

        private static string? NormalizePlanColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return null;

            return NormalizeRequiredHexColor(color);
        }

        private static string NormalizeRequiredHexColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return string.Empty;

            string trimmed = color.Trim().ToUpperInvariant();
            if (trimmed.Length != 7 || trimmed[0] != '#')
                return string.Empty;

            for (int index = 1; index < trimmed.Length; index++)
            {
                char symbol = trimmed[index];
                bool isDigit = symbol >= '0' && symbol <= '9';
                bool isUpperHex = symbol >= 'A' && symbol <= 'F';

                if (!isDigit && !isUpperHex)
                    return string.Empty;
            }

            return trimmed;
        }

        private static string BuildFullName(string? lastName, string? firstName, string? patronymic)
        {
            string ln = lastName ?? string.Empty;
            string fn = firstName ?? string.Empty;
            string pn = patronymic ?? string.Empty;
            return string.Join(" ", new[] { ln, fn, pn }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        }
    }
}
