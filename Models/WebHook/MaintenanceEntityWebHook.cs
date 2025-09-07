namespace CRMService.Models.WebHook
{
    public class MaintenanceEntityWebHook
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}
