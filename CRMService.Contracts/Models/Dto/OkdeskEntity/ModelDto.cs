using System.Text.Json.Serialization;

namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class ModelDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool Visible { get; set; }

        [JsonPropertyName("equipment_kind")]
        public KindDto? Kind { get; set; }

        [JsonPropertyName("equipment_manufacturer")]
        public ManufacturerDto? Manufacturer { get; set; }
    }
}



