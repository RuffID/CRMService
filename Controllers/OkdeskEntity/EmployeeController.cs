using CRMService.Interfaces.Repository;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IUnitOfWork unitOfWork, EmployeeService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetEmployee([FromQuery] int id, CancellationToken ct)
        {
            Employee? employee = await unitOfWork.Employee.GetItemByIdAsync(id, true, ct: ct);

            if (employee == null)
                return NotFound();

            return Ok(employee.ToDto());
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetEmployees([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(employees.ToDto());
        }

        [HttpGet("connections_with_group")]
        public async Task<IActionResult> GetGroupEmployeesConnections([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<EmployeeGroup> connections = await unitOfWork.EmployeeGroup.GetItemsByPredicateAsync(skip: skip, take: limit, asNoTracking: true, ct: ct);

            return Ok(connections.ToDto());
        }

        [HttpGet("by_group")]
        public async Task<IActionResult> GetEmployeesByGroup([FromQuery] int groupId, [FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.EmployeeGroups.Any(eg => eg.GroupId == groupId) && e.Id >= startIndexEmployee, asNoTracking: true, ct: ct);

            return Ok(employees.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudApi([FromQuery] long startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudApi(startIndexEmployee, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudDb([FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudDb(startIndexEmployee, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

            return NoContent();
        }
    }
}