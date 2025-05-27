using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class Manufacturer : IEntity
    {
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool? Visible { get; set; }

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