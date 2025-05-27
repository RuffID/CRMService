using CRMService.Interfaces.Repository;
using CRMService.Models.Entity;
using CRMService.Models.Report;

namespace CRMService.Service.Report
{
    public class ReportService(IUnitOfWorkEntities unitOfWork)
    {
        public async Task<List<ReportInfo>?> GetFullReportOnEmployees(DateTime dateFrom, DateTime dateTo, CancellationToken cancellation)
        {
            IEnumerable<Employee>? employees = await unitOfWork.Employee.GetItems(startIndex: 0, limit: await unitOfWork.Employee.GetCountOfItems());

            if (employees == null || !employees.Any())
                return null;

            List<ReportInfo> reportInfo = [];

            foreach (Employee employee in employees.Where(e=> e.Active == true))
            {
                ReportInfo report = new();
                report.EmployeeId = employee.Id;
                report.Issues = await unitOfWork.Report.GetInfoForOpenIssuesByEmployee(dateFrom, dateTo, employee.Id, cancellation);
                report.SolvedIssues = await unitOfWork.Report.GetSolvedIssuesByEmployee(dateFrom, dateTo, employee.Id, cancellation);
                report.SpentedTime = await unitOfWork.Report.GetSpentedTimeByEmployee(dateFrom, dateTo, employee.Id, cancellation);

                if (report.Issues == null && report.SolvedIssues == null && report.SpentedTime == null)
                    continue;

                reportInfo.Add(report);
            }

            return reportInfo;
        }

        public async Task<IssueInfo[]?> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellation)
        {
            return await unitOfWork.Report.GetArraySolvedIssuesByEmployee(dateFrom, dateTo, employeeId, cancellation);
        }
    }
}
