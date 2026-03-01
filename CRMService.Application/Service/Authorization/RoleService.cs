using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Application.Common.Mapping.Authorize;

namespace CRMService.Application.Service.Authorization
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



