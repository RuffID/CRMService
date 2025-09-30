using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class EmployeeMapping
    {
        public static IEnumerable<EmployeeDto> ToDto(this IEnumerable<Employee> employees)
        {
            foreach (Employee employee in employees)
                yield return employee.ToDto();
        }

        public static EmployeeDto ToDto(this Employee employee)
        {
            return new EmployeeDto()
            {
                Id = employee.Id,
                LastName = employee.LastName,
                FirstName = employee.FirstName,
                Patronymic = employee.Patronymic
            };
        }
    }
}
