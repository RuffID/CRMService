namespace CRMService.Contracts.Models.Dto.Report
{
    public class TimeChartSeriesDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<double> Values { get; set; } = new();
    }
}