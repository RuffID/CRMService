using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class EmployeeGroupMapping
    {
        public static IEnumerable<EmployeeGroupDto> ToDto(this IEnumerable<EmployeeGroup> employeeGroups)
        {
            foreach (EmployeeGroup employeeGroup in employeeGroups)
                yield return employeeGroup.ToDto();
        }

        public static EmployeeGroupDto ToDto(this EmployeeGroup employeeGroup)
        {
            return new EmployeeGroupDto()
            {
                EmployeeId = employeeGroup.EmployeeId,
                GroupId = employeeGroup.GroupId
            };
        }
    }
}



