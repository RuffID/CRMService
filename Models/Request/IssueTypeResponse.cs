using System.Text.Json.Serialization;

namespace CRMService.Models.Request
{
    public class IssueTypeResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool Default { get; set; }

        public bool Inner { get; set; }

        [JsonPropertyName("available_for_client")]

        public bool AvailableForClient { get; set; }

        public string Type { get; set; } = string.Empty;

        public List<IssueTypeResponse>? Children { get; set; }
    }
}
