namespace CRMService.Models.Authorization
{
    public class Equipment
    {
        public Guid Guid { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public Equipment()
        {
            Guid = Guid.NewGuid();
        }
    }
}
