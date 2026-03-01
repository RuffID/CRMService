using EFCoreLibrary.Abstractions.Entity;
using System.Text.Json.Serialization;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class Equipment : IEntity<int>, ICopyable<Equipment>
    {
        public int Id { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("inventory_number")]
        public string? InventoryNumber { get; set; }

        public int? KindId { get; set; }

        public int? ManufacturerId { get; set; }

        public int? ModelId { get; set; }

        public int? CompanyId { get; set; }

        public int? MaintenanceEntitiesId { get; set; }

        public virtual Company? Company { get; set; }

        [JsonPropertyName("equipment_kind")]
        public virtual Kind? Kind { get; set; }

        [JsonPropertyName("maintenance_entity")]
        public virtual MaintenanceEntity? MaintenanceEntities { get; set; }

        [JsonPropertyName("equipment_manufacturer")]
        public virtual Manufacturer? Manufacturer { get; set; }

        [JsonPropertyName("equipment_model")]
        public virtual Model? Model { get; set; }

        public virtual List<EquipmentParameter> Parameters { get; set; } = new List<EquipmentParameter>();

        public void CopyData(Equipment newItem)
        {
            SerialNumber = newItem.SerialNumber;
            InventoryNumber = newItem.InventoryNumber;
            KindId = newItem.KindId;
            ManufacturerId = newItem.ManufacturerId;
            ModelId = newItem.ModelId;
            CompanyId = newItem.CompanyId;
            MaintenanceEntitiesId = newItem.MaintenanceEntitiesId;
        }
    }
}


