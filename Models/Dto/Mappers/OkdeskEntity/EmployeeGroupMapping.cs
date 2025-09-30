using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
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
