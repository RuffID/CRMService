namespace CRMService.Web.Models.Server
{
    public class ServerData
    {
        public string? ServerName { get; set; }
        public DateTime ServerStartingTime { get; private set; }
        public string? ServerUpTime { get; set; }      

        public ServerData()
        {
            ServerName = "CRMService Dashboard";
            ServerStartingTime = DateTime.UtcNow;
        }
    }
}