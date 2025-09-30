using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;

namespace CRMService.Models.Dto.Mappers.Authorize
{
    public static class UserRoleMapping
    {
        public static IEnumerable<UserRoleDto> ToDto(this IEnumerable<UserRole> userRoles)
        {
            foreach (UserRole userRole in userRoles)
                yield return userRole.ToDto();
        }

        public static UserRoleDto ToDto(this UserRole userRole)
        {
            return new UserRoleDto()
            {
                UserId = userRole.UserId,
                RoleId = userRole.RoleId
            };
        }
    }
}
