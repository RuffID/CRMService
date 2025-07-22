using Newtonsoft.Json;

namespace CRMService.Models.Server
{
    public class ServerData
    {
        [JsonProperty]
        public string? ServerName { get; set; }
        [JsonProperty]
        public DateTime ServerStartingTime { get; private set; }
        [JsonProperty]
        public string? ServerUpTime { get; set; }      

        public ServerData()
        {
            ServerName = "CRMService Dashboard";
            ServerStartingTime = DateTime.UtcNow;
        }
    }
}
