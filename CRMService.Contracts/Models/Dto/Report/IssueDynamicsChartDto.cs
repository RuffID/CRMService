namespace CRMService.Contracts.Models.Dto.Report
{
    public class IssueDynamicsChartDto
    {
        public string Granularity { get; set; } = string.Empty;

        public List<DateTime> Buckets { get; set; } = new();

        public List<int> CreatedValues { get; set; } = new();

        public List<int> CompletedValues { get; set; } = new();
    }
}