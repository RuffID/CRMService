using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class EquipmentMapping
    {
        public static IEnumerable<EquipmentDto> ToDto(this IEnumerable<Equipment> equipments)
        {
            foreach (Equipment equipment in equipments)
                yield return equipment.ToDto();
        }

        public static EquipmentDto ToDto(this Equipment equipment)
        {
            return new EquipmentDto()
            {
                Id = equipment.Id,
                Serial_number = equipment.SerialNumber,
                Inventory_number = equipment.InventoryNumber,
                Kind = equipment.Kind == null ? null : new KindDto()
                {
                    Id = equipment.Kind.Id,
                    Name = equipment.Kind.Name,
                    Code = equipment.Kind.Code
                },
                Manufacturer = equipment.Manufacturer == null ? null : new ManufacturerDto()
                {
                    Id = equipment.Manufacturer.Id,
                    Name = equipment.Manufacturer.Name,
                    Code = equipment.Manufacturer.Code
                },
                Model = equipment.Model == null ? null : new ModelDto()
                {
                    Id = equipment.Model.Id,
                    Name = equipment.Model.Name,
                    Code = equipment.Model.Code
                },
                MaintenanceEntity = equipment.MaintenanceEntities == null ? null : new MaintenanceEntityDto()
                {
                    Id = equipment.MaintenanceEntities.Id,
                    Name = equipment.MaintenanceEntities.Name,
                    Address = equipment.MaintenanceEntities.Address,
                    Active = equipment.MaintenanceEntities.Active
                },
                Company = equipment.Company == null ? null : new CompanyDto()
                {
                    Id = equipment.Company.Id,
                    Name = equipment.Company.Name,
                    AdditionalName = equipment.Company.AdditionalName,
                    Active = equipment.Company.Active
                },
                Parameters = equipment.Parameters?.Select(p => new EquipmentParameterDto { Value = p.Value }).ToList()
            };
        }
    }
}
