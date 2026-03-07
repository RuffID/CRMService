using EFCoreLibrary.Abstractions.Entity;
using System.Text.Json.Serialization;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class Model : IEntity<int>, ICopyable<Model>
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool Visible { get; set; }

        public int? KindId { get; set; }

        public int? ManufacturerId { get; set; }

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

        [JsonPropertyName("equipment_kind")]
        public virtual Kind? Kind { get; set; }

        [JsonPropertyName("equipment_manufacturer")]
        public virtual Manufacturer? Manufacturer { get; set; }

        public void CopyData(Model model)
        {
            Code = model.Code;
            Name = model.Name;
            Description = model.Description;
            Visible = model.Visible;
            KindId = model.KindId;
            ManufacturerId = model.ManufacturerId;
        }
    }
}