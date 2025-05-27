using Newtonsoft.Json;

namespace CRMService.Models.Server
{
    public class ServerData
    {
        [JsonProperty]
        public string? ServerName { get; set; }
        [JsonProperty]
        public string? ServerStartingTime { get; set; }
        [JsonProperty]
        public string? ServerUpTime { get; set; }      
    }
}
