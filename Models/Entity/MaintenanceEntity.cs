using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Entity
{
    public class MaintenanceEntity : IEntity<int>, ICopyable<MaintenanceEntity>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        public bool Active { get; set; }

        [JsonProperty("Company_id")]
        public int CompanyId { get; set; }

        public virtual Company Company { get; set; } = null!;

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public void CopyData(MaintenanceEntity newItem)
        {
            Name = newItem.Name;
            Address = newItem.Address;
            Active = newItem.Active;
            CompanyId = newItem.CompanyId;
        }
    }
}