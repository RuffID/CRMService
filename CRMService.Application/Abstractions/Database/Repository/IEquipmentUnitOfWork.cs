using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IEquipmentUnitOfWork : IUnitOfWorkScope
    {
        IEquipmentRepository Equipment { get; }
        IParameterRepository Parameter { get; }
        IKindRepository Kind { get; }
        IKindParamsRepository KindParams { get; }
        IKindParameterRepository KindParameter { get; }
        IMaintenanceEntityRepository MaintenanceEntity { get; }
        IManufacturerRepository Manufacturer { get; }
        IModelRepository Model { get; }
        ICompanyRepository Company { get; }
    }
}
