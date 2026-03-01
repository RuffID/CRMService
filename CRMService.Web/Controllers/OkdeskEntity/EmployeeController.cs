using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Web.Controllers.OkdeskEntity
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
        public async Task<IActionResult> GetEmployees(CancellationToken ct = default)
        {
            return JsonResultMapper.ToJsonResult(await service.GetEmployees(ct: ct));
        }

        [HttpGet("connections_with_group")]
        public async Task<IActionResult> GetGroupEmployeesConnections(CancellationToken ct = default)
        {
            List<EmployeeGroup> connections = await unitOfWork.EmployeeGroup.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(connections.ToDto());
        }

        [HttpGet("by_group")]
        public async Task<IActionResult> GetEmployeesByGroup([FromQuery] int groupId, CancellationToken ct)
        {
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.EmployeeGroups.Any(eg => eg.GroupId == groupId), asNoTracking: true, ct: ct);

            return Ok(employees.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudDb([FromQuery] int startIndexEmployee, CancellationToken ct)
        {
            await service.UpdateEmployeesFromCloudDb(ct);

            return NoContent();
        }
    }
}