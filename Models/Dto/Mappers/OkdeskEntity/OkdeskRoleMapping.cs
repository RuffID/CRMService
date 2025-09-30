using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class OkdeskRoleMapping
    {
        public static IEnumerable<OkdeskRoleDto> ToDto(this IEnumerable<OkdeskRole> roles)
        {
            foreach (OkdeskRole role in roles)
                yield return role.ToDto();
        }

        public static OkdeskRoleDto ToDto(this OkdeskRole role)
        {
            return new OkdeskRoleDto()
            {
                Id = role.Id,
                Name = role.Name,
            };
        }
    }
}
