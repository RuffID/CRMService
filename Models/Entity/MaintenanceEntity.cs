using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Entity
{
    public class MaintenanceEntity : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public sbyte? Active { get; set; }

        [JsonProperty("Company_id")]
        public int? CompanyId { get; set; }

        public virtual Company? Company { get; set; }

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }
}