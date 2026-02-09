using CRMService.Abstractions.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class Manufacturer : IEntity<int>, IHasCode, ICopyable<Manufacturer>
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool Visible { get; set; }

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

        public virtual ICollection<Model> Models { get; set; } = new List<Model>();

        public void CopyData(Manufacturer manufacturer)
        {
            Code = manufacturer.Code;
            Name = manufacturer.Name;
            Description = manufacturer.Description;
            Visible = manufacturer.Visible;
        }
    }
}