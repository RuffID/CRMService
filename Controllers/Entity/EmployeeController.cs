using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using CRMService.Dto;
using CRMService.Core;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class EmployeeController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IOptions<OkdeskSettings> okdSettings, IUnitOfWorkEntities unitOfWork, EmployeeService service) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetEmployee([FromQuery] int id)
        {
            EmployeeDto? employee = mapper.Map<EmployeeDto>(await unitOfWork.Employee.GetItem(new Employee() { Id = id }, false));

            if (employee == null)
                return NotFound("Employee not found.");

            return Ok(employee);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetEmployees([FromQuery] int startIndex = 0)
        {
            IEnumerable<EmployeeDto>? employees = mapper.Map<IEnumerable<EmployeeDto>>(await unitOfWork.Employee.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (employees == null || !employees.Any())
                return NotFound();

            return Ok(employees);
        }

        [HttpGet("connections_with_group")]
        public async Task<IActionResult> GetGroupEmployeesConnections([FromQuery] int startIndex = 0)
        {
            IEnumerable<EmployeeGroup>? connections = await unitOfWork.EmployeeGroup.GetItems(startIndex, await unitOfWork.EmployeeGroup.GetCountOfItems());

            if (connections == null || !connections.Any())
                return NotFound();

            return Ok(connections);
        }

        [HttpGet("by_group")]
        public async Task<IActionResult> GetEmployeesByGroup([FromQuery] int groupId = 1, [FromQuery] int startIndexEmployee = 0)
        {
            IEnumerable<EmployeeDto>? employees = mapper.Map<IEnumerable<EmployeeDto>>(await unitOfWork.Employee.GetEmployeesByGroup(groupId, startIndexEmployee, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (employees == null || !employees.Any())
                return NotFound();

            return Ok(employees);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudApi([FromQuery] long startIndexEmployee = 0)
        {
            await service.UpdateEmployeesFromCloudApi(startIndexEmployee, okdSettings.Value.LimitForRetrievingEntitiesFromApi);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateEmployeesFromCloudDb([FromQuery] int startIndexEmployee = 0)
        {
            await service.UpdateEmployeesFromCloudDb(startIndexEmployee, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            return NoContent();
        }
    }
}
