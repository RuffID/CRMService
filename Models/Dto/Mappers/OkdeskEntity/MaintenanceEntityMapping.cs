using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.WebHook;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class MaintenanceEntityMapping
    {
        public static IEnumerable<MaintenanceEntityDto> ToDto(this IEnumerable<MaintenanceEntity> objects)
        {
            foreach (MaintenanceEntity entity in objects)
                yield return entity.ToDto();
        }

        public static MaintenanceEntityDto ToDto(this MaintenanceEntity maintenanceEntity)
        {
            return new MaintenanceEntityDto()
            {
                Id = maintenanceEntity.Id,
                Name = maintenanceEntity.Name,
                Address = maintenanceEntity.Address,
                Active = maintenanceEntity.Active
            };
        }

        public static MaintenanceEntity ToEntity(this MaintenanceEntityWebHook maintenanceEntity)
        {
            return new MaintenanceEntity()
            {
                Id = maintenanceEntity.Id,
                Name = maintenanceEntity.Name,
                Active = maintenanceEntity.Active
            };
        }
    }
}
