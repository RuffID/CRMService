using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;

namespace CRMService.Models.Dto.Mappers.Authorize
{
    public static class UserMapping
    {
        public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users)
        {
            foreach (User user in users)
                yield return user.ToDto();
        }

        public static UserDto ToDto(this User user)
        {
            return new UserDto()
            {
                Id = user.Id,
                Login = user.Login,
                Active = user.Active,
                Roles = user.Roles.Select(r => new CrmRoleDto()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };
        }
    }
}
