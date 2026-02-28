using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Dto.Mappers.Authorize;
using CRMService.Models.Responses.Results;

namespace CRMService.Service.Authorization
{
    public class RoleService(IUnitOfWork unitOfWork)
    {
        public async Task<ServiceResult<List<CrmRoleDto>>> GetRolesAsync(CancellationToken ct)
        {
            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<CrmRoleDto>>.Ok(roles.ToDto().ToList());
        }
    }
}
