using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IOkdeskEquipmentSource
    {
        IOkdeskKindRepository Kind { get; }
        IOkdeskKindParameterRepository KindParameter { get; }
        IOkdeskKindParamsRepository KindParams { get; }
        IOkdeskMaintenanceEntityRepository MaintenanceEntity { get; }
        IOkdeskManufacturerRepository Manufacturer { get; }
        IOkdeskModelRepository Model { get; }
        IOkdeskEquipmentRepository Equipment { get; }
    }
}
