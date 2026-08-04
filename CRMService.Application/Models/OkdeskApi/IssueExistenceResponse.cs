using System.Text.Json.Serialization;

namespace CRMService.Application.Models.OkdeskApi
{
    public class IssueExistenceResponse
    {
        public int Id { get; set; }

        [JsonPropertyName("errors")]
        public string? Errors { get; set; }
    }
}
