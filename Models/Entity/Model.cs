using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Entity
{
    public class Model : IEntity<int>, IHasCode, ICopyable<Model>
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public sbyte? Visible { get; set; }

        public int? KindId { get; set; }

        public int? ManufacturerId { get; set; }

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

        [JsonProperty("equipment_kind")]
        public virtual Kind? Kind { get; set; }

        [JsonProperty("equipment_manufacturer")]
        public virtual Manufacturer? Manufacturer { get; set; }

        public void CopyData(Model model)
        {
            Code = model.Code;
            Name = model.Name;
            Description = model.Description;
            Visible = model.Visible;
        }
    }
}

