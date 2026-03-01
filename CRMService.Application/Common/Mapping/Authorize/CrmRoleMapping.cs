using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;

namespace CRMService.Application.Common.Mapping.Authorize
{
    public static class CrmRoleMapping
    {
        public static IEnumerable<CrmRoleDto> ToDto(this IEnumerable<CrmRole> groups)
        {
            foreach (CrmRole group in groups)
                yield return group.ToDto();
        }

        public static CrmRoleDto ToDto(this CrmRole group)
        {
            return new CrmRoleDto()
            {
                Id = group.Id,
                Name = group.Name,
            };
        }

        public static CrmRole ToEntity(this CrmRoleDto group)
        {
            return new CrmRole()
            {
                Id = group.Id,
                Name = group.Name,
            };
        }
    }
}



