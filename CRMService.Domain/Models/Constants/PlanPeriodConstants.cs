namespace CRMService.Domain.Models.Constants
{
    public static class PlanPeriodConstants
    {
        public const string DAY = "day";
        public const string WEEK = "week";
        public const string MONTH = "month";
        public const string YEAR = "year";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DAY,
            WEEK,
            MONTH,
            YEAR
        };
    }
}



