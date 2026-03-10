namespace CRMService.Contracts.Models.Request
{
    public class IssueDynamicsChartRequest
    {
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string Granularity { get; set; } = string.Empty;
    }
}