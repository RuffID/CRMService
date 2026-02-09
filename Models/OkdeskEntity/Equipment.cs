using CRMService.Abstractions.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.OkdeskEntity
{
    public class Equipment : IEntity<int>, ICopyable<Equipment>
    {
        public int Id { get; set; }

        [JsonProperty("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonProperty("inventory_number")]
        public string? InventoryNumber { get; set; }

        public int? KindId { get; set; }

        public int? ManufacturerId { get; set; }

        public int? ModelId { get; set; }

        public int? CompanyId { get; set; }

        public int? MaintenanceEntitiesId { get; set; }

        public virtual Company? Company { get; set; }

        [JsonProperty("equipment_kind")]
        public virtual Kind? Kind { get; set; }

        [JsonProperty("maintenance_entity")]
        public virtual MaintenanceEntity? MaintenanceEntities { get; set; }

        [JsonProperty("equipment_manufacturer")]
        public virtual Manufacturer? Manufacturer { get; set; }

        [JsonProperty("equipment_model")]
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