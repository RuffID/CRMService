namespace CRMService.Contracts.Models.Request
{
    public class LookupListRequest
    {
        public string? Search { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; } = 20;
    }
}
