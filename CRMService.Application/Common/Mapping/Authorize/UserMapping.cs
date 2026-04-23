using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;
using CRMService.Domain.Models.OkdeskEntity;

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
                EmployeeId = user.EmployeeId,
                EmployeeName = user.Employee == null ? null : FormatEmployeeName(user.Employee),
                Roles = user.Roles.Select(r => new CrmRoleDto()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };
        }

        private static string FormatEmployeeName(Employee employee)
        {
            string[] parts = new[]
            {
                employee.LastName ?? string.Empty,
                employee.FirstName ?? string.Empty,
                employee.Patronymic ?? string.Empty
            };

            string fullName = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
            return string.IsNullOrWhiteSpace(fullName) ? $"#{employee.Id}" : fullName;
        }
    }
}



