using CRMService.Abstractions.Entity;

namespace CRMService.Models.OkdeskEntity;

public class CompanyCategory : IEntity<int>, IHasCode, ICopyable<CompanyCategory>
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public void CopyData(CompanyCategory newItem)
    {
        Code = newItem.Code;
        Name = newItem.Name;
        Color = newItem.Color;
    }
}