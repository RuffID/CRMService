namespace CRMService.Application.Models.Report
{
    public class TimeChartPointInfo
    {
        public int EntityId { get; set; }

        public DateTime BucketStart { get; set; }

        public double SpentedTime { get; set; }
    }
}