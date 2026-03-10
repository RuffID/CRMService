namespace CRMService.Contracts.Models.Dto.Report
{
    public class TimeChartDto
    {
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// Ось времени (например CreatedAt или LoggedAt, ориентация по времени создания или по времени которое указали в списании)
        /// </summary>
        public string TimeAxis { get; set; } = string.Empty;

        public string Granularity { get; set; } = string.Empty;

        public List<DateTime> Buckets { get; set; } = new();

        public List<TimeChartSeriesDto> Series { get; set; } = new();
    }
}