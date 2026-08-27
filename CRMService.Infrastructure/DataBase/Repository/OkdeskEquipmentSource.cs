using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class OkdeskEquipmentSource(
        IOkdeskKindRepository kind,
        IOkdeskKindParameterRepository kindParameter,
        IOkdeskKindParamsRepository kindParams,
        IOkdeskMaintenanceEntityRepository maintenanceEntity,
        IOkdeskManufacturerRepository manufacturer,
        IOkdeskModelRepository model,
        IOkdeskEquipmentRepository equipment) : IOkdeskEquipmentSource
    {
        public IOkdeskKindRepository Kind { get; } = kind;
        public IOkdeskKindParameterRepository KindParameter { get; } = kindParameter;
        public IOkdeskKindParamsRepository KindParams { get; } = kindParams;
        public IOkdeskMaintenanceEntityRepository MaintenanceEntity { get; } = maintenanceEntity;
        public IOkdeskManufacturerRepository Manufacturer { get; } = manufacturer;
        public IOkdeskModelRepository Model { get; } = model;
        public IOkdeskEquipmentRepository Equipment { get; } = equipment;
    }
}
