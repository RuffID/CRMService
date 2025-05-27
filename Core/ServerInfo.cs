namespace CRMService.Core
{
    public class ServerInfo
    {        
        public DateTime ServerStartingTime { get; }

        public ServerInfo()
        {
            ServerStartingTime = DateTime.Now;
            Console.WriteLine(">>> ServerInfo created: " + ServerStartingTime);
        }
    }
}
