using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Entity;

public class Company : IEntity<int>, ICopyable<Company>
{
    public int Id { get; set; }

    public string? Name { get; set; }

    [JsonProperty("additional_name")]
    public string? AdditionalName { get; set; }

    public bool? Active { get; set; }

    public int? CategoryId { get; set; }

    public virtual CompanyCategory? Category { get; set; }

    public virtual ICollection<Equipment>? Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<Issue>? Issues { get; set; } = new List<Issue>();

    public virtual ICollection<MaintenanceEntity>? MaintenanceEntities { get; set; } = new List<MaintenanceEntity>();

    public void CopyData(Company company)
    {
        Name = company.Name;
        AdditionalName = company.AdditionalName;
        Active = company.Active;
        CategoryId = company.CategoryId;
    }
}
