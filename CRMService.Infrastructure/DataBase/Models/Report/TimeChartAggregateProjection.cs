namespace CRMService.Infrastructure.DataBase.Models.Report
{
    public class TimeChartAggregateProjection
    {
        public int EntityId { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public int? Hour { get; set; }

        public int? Minute { get; set; }

        public double SpentedTime { get; set; }
    }
}