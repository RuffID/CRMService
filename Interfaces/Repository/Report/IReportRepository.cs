using CRMService.Models.Report;

namespace CRMService.Interfaces.Repository.Report
{
    public interface IReportRepository
    {
        Task<long> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken);
        Task<IssueInfo[]> GetArraySolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken);
        Task<double> GetSpentedTimeByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken);
        Task<IssueInfo[]> GetInfoForOpenIssuesByEmployee(DateTime dateFrom, DateTime dateTo, long employeeId, CancellationToken cancellationToken);
    }
}
