using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IMapper mapper, IUnitOfWork unitOfWork, EmployeeService service) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetEmployee([FromQuery] int id, CancellationToken ct)
        {
            Employee? employee = await unitOfWork.Employee.GetItemById(id, true, ct);

            if (employee == null)
                return NotFound();

            return Ok(mapper.Map<EmployeeDto>(employee));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetEmployees([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAndSortById(predicate: e => e.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<EmployeeDto>>(employees));
        }

        [HttpGet("connections_with_group")]
        public async Task<IActionResult> GetGroupEmployeesConnections([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<EmployeeGroup> connections = await unitOfWork.EmployeeGroup.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            return Ok(connections);
        }

        [HttpGet("by_group")]
        public async Task<IActionResult> GetEmployeesByGroup([FromQuery] int groupId, [FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            List<Employee> employees = await unitOfWork.EmployeeGroup.GetEmployeesByGroup(groupId, startIndexEmployee, true, ct);

            return Ok(mapper.Map<List<EmployeeDto>>(employees));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateEmployeesFromCloudApi([FromQuery] long startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudApi(startIndexEmployee, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateEmployeesFromCloudDb([FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudDb(startIndexEmployee, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

            return NoContent();
        }
    }
}
