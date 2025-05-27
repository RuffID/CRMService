using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity;

public class CompanyCategory : IEntity
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Color { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
