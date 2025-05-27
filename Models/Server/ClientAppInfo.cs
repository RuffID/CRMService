namespace CRMService.Models.Server
{
    public class ClientAppInfo
    {
        public int Id { get; set; }
        public string? Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Description { get; set; }
    }
}