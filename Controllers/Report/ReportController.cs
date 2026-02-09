using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Report
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController(/*ReportService service*/) : Controller
    {
        /*[HttpGet]
        public async Task<IActionResult> GetFullReport([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
        {
            if (dateFrom > dateTo)
                return BadRequest("The start date of the period cannot be greater than the end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            List<ReportInfo>? reportInfo = await service.GetFullReportOnEmployees(dateFrom, dateTo, HttpContext.RequestAborted);

            if (reportInfo == null || reportInfo.Count == 0)
            {
                return NotFound($"Data from {dateFrom:dd.MM.yyyy} to {dateTo:dd.MM.yyyy} not found.");
            }

            return Ok(reportInfo);
        }

        [HttpGet("solved_issues_by_employee")]
        public async Task<IActionResult> GetArrayOfSolvedIssues([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] int employeeId)
        {
            if (dateFrom > dateTo)
                return BadRequest("The start date of the period cannot be greater than the end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            IssueInfo[]? reportInfo = await service.GetSolvedIssuesByEmployee(dateFrom, dateTo, employeeId, HttpContext.RequestAborted);

            if (reportInfo == null || reportInfo.Length == 0)
            {
                return NotFound($"Data from {dateFrom:dd.MM.yyyy} to {dateTo:dd.MM.yyyy} by employee id {employeeId} not found.");
            }

            return Ok(reportInfo);
        }*/
    }
}
