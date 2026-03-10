namespace CRMService.Infrastructure.DataBase.Models.Report
{
    public class IssueDynamicsAggregateProjection
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public int Hour { get; set; }

        public int Count { get; set; }
    }
}