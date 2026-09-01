using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Entity;

public partial class CategoryRepository
{
    public Task<CompanyCategory?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<CompanyCategory?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<CompanyCategory>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<CompanyCategory>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<CompanyCategory?> GetByCodeAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, ct: ct);
    public Task<CompanyCategory?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
}

public partial class CompanyRepository
{
    public Task<Company?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Company?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Company>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Company>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<Company>> GetByCategoryCodeReadOnlyAsync(string categoryCode, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.Category != null && x.Category.Code == categoryCode, asNoTracking: true, ct: ct);
    public Task<List<Company>> SearchReadOnlyAsync(string? search, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => search == null || x.Name.Contains(search), asNoTracking: true, ct: ct);
    public Task<List<Company>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), asNoTracking: true, ct: ct);
}

public partial class EmployeeRepository
{
    public Task<Employee?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemByid.GetItemByIdAsync(id, ct: ct);
    public Task<Employee?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemByid.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Employee>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Employee>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<Employee>> GetWithGroupsReadOnlyAsync(IReadOnlyCollection<int>? groupIds, bool includeInactive, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => (includeInactive || x.Active) && (groupIds == null || x.EmployeeGroups.Any(g => groupIds.Contains(g.GroupId))), asNoTracking: true, include: q => q.Include(x => x.EmployeeGroups).ThenInclude(x => x.Group), ct: ct);
    public Task<List<Employee>> SearchReadOnlyAsync(string? search, bool activeOnly, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => (!activeOnly || x.Active) && (search == null || (x.LastName != null && x.LastName.Contains(search)) || (x.FirstName != null && x.FirstName.Contains(search)) || (x.Patronymic != null && x.Patronymic.Contains(search))), asNoTracking: true, ct: ct);
    public Task<List<Employee>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, bool activeOnly = false, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id) && (!activeOnly || x.Active), asNoTracking: true, ct: ct);
    public Task<List<Employee>> GetActiveFromIdReadOnlyAsync(long startId, bool inclusive, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.Active && (inclusive ? x.Id >= startId : x.Id > startId), asNoTracking: true, ct: ct);
}

public partial class EmployeeGroupRepository
{
    public Task<List<EmployeeGroup>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<EmployeeGroup>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<EmployeeGroup>> GetByGroupIdsReadOnlyAsync(IReadOnlyCollection<int> groupIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => groupIds.Contains(x.GroupId), asNoTracking: true, ct: ct);
}

public partial class EmployeeRoleRepository
{
    public Task<List<EmployeeRole>> GetByEmployeeIdsReadOnlyAsync(IReadOnlyCollection<int> employeeIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => employeeIds.Contains(x.EmployeeId), asNoTracking: true, ct: ct);
}

public partial class EquipmentRepository
{
    public Task<Equipment?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Equipment?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<Equipment?> GetWithParametersReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, q => q.Include(x => x.Parameters), ct);
    public Task<List<Equipment>> GetByMaintenanceEntityWithParametersReadOnlyAsync(int maintenanceEntityId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.MaintenanceEntitiesId == maintenanceEntityId, asNoTracking: true, include: q => q.Include(x => x.Parameters), ct: ct);
    public Task<List<Equipment>> GetByCompanyWithParametersReadOnlyAsync(int companyId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.CompanyId == companyId, asNoTracking: true, include: q => q.Include(x => x.Parameters), ct: ct);
    public Task<Equipment?> GetDetailsReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, q => q
        .Include(x => x.Company).ThenInclude(x => x!.Category)
        .Include(x => x.Kind).ThenInclude(x => x!.KindParams).ThenInclude(x => x.KindParameter)
        .Include(x => x.Manufacturer)
        .Include(x => x.Model)
        .Include(x => x.MaintenanceEntities)
        .Include(x => x.Parameters).ThenInclude(x => x.KindParameter), ct);
}

public partial class GroupRepository
{
    public Task<Group?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemByid.GetItemByIdAsync(id, ct: ct);
    public Task<Group?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemByid.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Group>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Group>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<Group>> SearchReadOnlyAsync(string? search, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => search == null || x.Name.Contains(search), asNoTracking: true, ct: ct);
    public Task<List<Group>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), asNoTracking: true, ct: ct);
}

public partial class IssuePriorityRepository
{
    public Task<IssuePriority?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<IssuePriority?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<IssuePriority>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<IssuePriority>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<IssuePriority?> GetByCodeAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, ct: ct);
    public Task<IssuePriority?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<IssuePriority>> SearchReadOnlyAsync(string? search, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => search == null || (x.Name != null && x.Name.Contains(search)) || x.Code.Contains(search), asNoTracking: true, ct: ct);
    public Task<List<IssuePriority>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class IssueRepository
{
    public Task<Issue?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Issue?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<Issue?> GetDetailsReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, q => q
        .Include(x => x.Company).ThenInclude(x => x!.Category)
        .Include(x => x.ServiceObject)
        .Include(x => x.Assignee)
        .Include(x => x.Priority)
        .Include(x => x.Status)
        .Include(x => x.Type), ct);
    public Task<List<Issue>> GetFromIdReadOnlyAsync(int startId, int limit, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.Id >= startId, take: limit, asNoTracking: true, ct: ct);
    public Task<List<Issue>> GetUpdatedLocalReadOnlyAsync(DateTime dateFrom, DateTime dateTo, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.DeletedAt == null && x.Id >= 0 && x.EmployeesUpdatedAt >= dateFrom && x.EmployeesUpdatedAt <= dateTo, asNoTracking: true, ct: ct);
    public Task<List<Issue>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), ct: ct);
}

public partial class IssueStatusRepository
{
    public Task<IssueStatus?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<IssueStatus?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<IssueStatus>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<IssueStatus>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<IssueStatus?> GetByCodeAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, ct: ct);
    public Task<IssueStatus?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<IssueStatus>> SearchReadOnlyAsync(string? search, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => search == null || x.Name.Contains(search) || x.Code.Contains(search), asNoTracking: true, ct: ct);
    public Task<List<IssueStatus>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class IssueTypeRepository
{
    public Task<IssueType?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<IssueType?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<IssueType>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<IssueType>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<IssueType?> GetByCodeAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, ct: ct);
    public Task<IssueType?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<IssueType>> GetAllWithGroupReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, include: q => q.Include(x => x.Group), ct: ct);
    public Task<List<IssueType>> SearchReadOnlyAsync(string? search, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => search == null || x.Name.Contains(search) || x.Code.Contains(search), asNoTracking: true, ct: ct);
    public Task<List<IssueType>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class IssueTypeGroupRepository
{
    public Task<IssueTypeGroup?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<IssueTypeGroup?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<IssueTypeGroup>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<IssueTypeGroup>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<IssueTypeGroup?> GetByCodeAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, ct: ct);
}

public partial class KindParameterRepository
{
    public Task<KindsParameter?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<KindsParameter?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<KindsParameter>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<KindsParameter>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<KindsParameter?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<KindsParameter>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), asNoTracking: true, ct: ct);
    public Task<List<KindsParameter>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class KindParamsRepository
{
    public Task<List<KindParam>> GetByKindIdsReadOnlyAsync(IReadOnlyCollection<int> kindIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => kindIds.Contains(x.KindId), asNoTracking: true, ct: ct);
}

public partial class KindRepository
{
    public Task<Kind?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Kind?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Kind>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Kind>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<Kind?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<Kind>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? modelIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => (search == null || x.Name.Contains(search) || x.Code.Contains(search)) && (modelIds == null || x.Models.Any(m => modelIds.Contains(m.Id))), asNoTracking: true, ct: ct);
    public Task<List<Kind>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), asNoTracking: true, ct: ct);
    public Task<List<Kind>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class MaintenanceEntityRepository
{
    public Task<MaintenanceEntity?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<MaintenanceEntity?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<MaintenanceEntity>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<MaintenanceEntity>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<MaintenanceEntity>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? companyIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => (search == null || x.Name.Contains(search)) && (companyIds == null || (x.CompanyId.HasValue && companyIds.Contains(x.CompanyId.Value))), asNoTracking: true, ct: ct);
    public Task<List<MaintenanceEntity>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), asNoTracking: true, ct: ct);
}

public partial class ManufacturerRepository
{
    public Task<Manufacturer?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Manufacturer?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Manufacturer>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Manufacturer>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<Manufacturer?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<Manufacturer>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? modelIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => (search == null || x.Name.Contains(search) || x.Code.Contains(search)) && (modelIds == null || x.Models.Any(m => modelIds.Contains(m.Id))), asNoTracking: true, ct: ct);
    public Task<List<Manufacturer>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class ModelRepository
{
    public Task<Model?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Model?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Model>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Model>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<Model?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Code == code, true, ct: ct);
    public Task<List<Model>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? kindIds, IReadOnlyCollection<int>? manufacturerIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => (kindIds == null || (x.KindId.HasValue && kindIds.Contains(x.KindId.Value))) && (manufacturerIds == null || (x.ManufacturerId.HasValue && manufacturerIds.Contains(x.ManufacturerId.Value))) && (search == null || x.Name.Contains(search) || (x.Code != null && x.Code.Contains(search)) || (x.Manufacturer != null && x.Manufacturer.Name.Contains(search))), asNoTracking: true, include: q => q.Include(x => x.Manufacturer), ct: ct);
    public Task<List<Model>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.Code != null && codes.Contains(x.Code), asNoTracking: true, ct: ct);
}

public partial class OkdeskRoleRepository
{
    public Task<OkdeskRole?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemByid.GetItemByIdAsync(id, ct: ct);
    public Task<OkdeskRole?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemByid.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<OkdeskRole>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<OkdeskRole>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class ParameterRepository
{
    public Task<List<EquipmentParameter>> GetByEquipmentIdAsync(int equipmentId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.EquipmentId == equipmentId, ct: ct);
}

public partial class TimeEntryRepository
{
    public Task<TimeEntry?> GetItemByIdAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<TimeEntry?> GetItemByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<TimeEntry>> GetByIssueIdAsync(int issueId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.IssueId == issueId, ct: ct);
    public Task<List<TimeEntry>> GetByIssueIdReadOnlyAsync(int issueId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.IssueId == issueId, asNoTracking: true, ct: ct);
    public Task<List<TimeEntry>> GetMissingFromCloudAsync(int issueId, IReadOnlyCollection<int> cloudIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.IssueId == issueId && !cloudIds.Contains(x.Id), ct: ct);
}
