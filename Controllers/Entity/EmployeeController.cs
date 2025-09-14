using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IUnitOfWork unitOfWork, EmployeeService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetEmployee([FromQuery] int id, CancellationToken ct)
        {
            Employee? employee = await unitOfWork.Employee.GetItemById(id, true, ct);

            if (employee == null)
                return NotFound();

            EmployeeDto dto = new()
            {
                Id = employee.Id,
                LastName = employee.LastName,
                FirstName = employee.FirstName,
                Patronymic = employee.Patronymic
            };

            return Ok(dto);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetEmployees([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAndSortById(predicate: e => e.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            List<EmployeeDto> dtos = employees.Select(e => new EmployeeDto()
            {
                Id = e.Id,
                LastName = e.LastName,
                FirstName = e.FirstName,
                Patronymic = e.Patronymic
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("connections_with_group")]
        public async Task<IActionResult> GetGroupEmployeesConnections([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<EmployeeGroup> connections = await unitOfWork.EmployeeGroup.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            List<EmployeeGroupDto> dtos = connections.Select(eg => new EmployeeGroupDto()
            {
                EmployeeId = eg.EmployeeId,
                GroupId = eg.GroupId
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("by_group")]
        public async Task<IActionResult> GetEmployeesByGroup([FromQuery] int groupId, [FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            List<Employee> employees = await unitOfWork.EmployeeGroup.GetEmployeesByGroup(groupId, startIndexEmployee, true, ct);

            List<EmployeeDto> dtos = employees.Select(e => new EmployeeDto()
            {
                Id = e.Id,
                LastName = e.LastName,
                FirstName = e.FirstName,
                Patronymic = e.Patronymic
            }).ToList();

            return Ok(dtos);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudApi([FromQuery] long startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudApi(startIndexEmployee, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudDb([FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudDb(startIndexEmployee, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

            return NoContent();
        }
    }
}