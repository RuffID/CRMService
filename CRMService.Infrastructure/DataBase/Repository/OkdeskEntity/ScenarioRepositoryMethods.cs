using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.OkdeskSource;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity;

public partial class OkdeskCompanyCategoryRepository
{
    public Task<List<CompanyCategory>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskCompanyRepository
{
    public Task<List<Company>> GetAllWithCategoryReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, include: q => q.Include(x => x.Category), ct: ct);
}

public partial class OkdeskEmployeeRepository
{
    public Task<List<Employee>> GetEmployeesReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => EF.Property<string>(x, "Type") == "Employee", asNoTracking: true, ct: ct);
    public Task<List<Employee>> GetContactsByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id) && EF.Property<string>(x, "Type") == "Contact", asNoTracking: true, ct: ct);
    public Task<Employee?> GetContactByIdReadOnlyAsync(int id, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Id == id && EF.Property<string>(x, "Type") == "Contact", true, ct: ct);
}

public partial class OkdeskGroupRepository
{
    public Task<List<Group>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskIssuePriorityRepository
{
    public Task<List<IssuePriority>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskIssueStatusRepository
{
    public Task<List<IssueStatus>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskIssueTypeRepository
{
    public Task<List<IssueType>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskIssueTypeGroupRepository
{
    public Task<List<IssueTypeGroup>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskKindRepository
{
    public Task<List<Kind>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskKindParameterRepository
{
    public Task<List<OkdeskKindParameterRecord>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskKindParamsRepository
{
    public Task<List<OkdeskKindParameterConnectionRecord>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskMaintenanceEntityRepository
{
    public Task<List<MaintenanceEntity>> GetAllWithCompanyReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, include: q => q.Include(x => x.Company), ct: ct);
}

public partial class OkdeskManufacturerRepository
{
    public Task<List<Manufacturer>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class OkdeskModelRepository
{
    public Task<List<Model>> GetAllReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}
