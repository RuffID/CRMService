using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Dto.OkdeskEntity
{
    public class ModelDto : IEntity<int>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool Visible { get; set; }

        [JsonProperty("equipment_kind")]
        public KindDto? Kind { get; set; }

        [JsonProperty("equipment_manufacturer")]
        public ManufacturerDto? Manufacturer { get; set; }
    }
}
