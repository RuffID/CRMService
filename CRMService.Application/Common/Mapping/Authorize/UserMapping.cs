using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;

namespace CRMService.Application.Common.Mapping.Authorize
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
                Name = user.Name,
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



