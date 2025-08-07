namespace CRMService.Dto.Entity
{
    public class EquipmentDto
    {
        public int Id { get; set; }

        public string? Serial_number { get; set; }

        public string? Inventory_number { get; set; }

        public IEnumerable<EquipmentParameterDto>? Parameters { get; set; } = [];

        public KindDto? Kind { get; set; }

        public ManufacturerDto? Manufacturer { get; set; }

        public ModelDto? Model { get; set; }
    }
}