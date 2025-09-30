using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Report;

namespace CRMService.Service.Report
{
    public class ReportService(IUnitOfWork unitOfWork)
    {
        public async Task<List<ReportInfo>?> GetFullReportOnEmployees(DateTime dateFrom, DateTime dateTo, CancellationToken cancellation)
        {
            List<Employee>? employees = await unitOfWork.Employee.GetItemsByPredicateAndSortById(asNoTracking: true, ct: cancellation);

            if (employees == null || employees.Count == 0)
                return null;

            List<ReportInfo> reportInfo = [];

            foreach (Employee employee in employees.Where(e=> e.Active == true))
            {
                if (cancellation.IsCancellationRequested)
                    break;

                ReportInfo report = new();
                report.EmployeeId = employee.Id;
                report.Issues = await unitOfWork.Report.GetInfoForOpenIssuesByEmployee(dateFrom, dateTo, employee.Id, cancellation);
                report.SolvedIssues = await unitOfWork.Report.GetSolvedIssuesByEmployee(dateFrom, dateTo, employee.Id, cancellation);
                report.SpentedTime = await unitOfWork.Report.GetSpentedTimeByEmployee(dateFrom, dateTo, employee.Id, cancellation);

                if (report.Issues.Length == 0 && report.SolvedIssues == 0 && report.SpentedTime == 0)
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
