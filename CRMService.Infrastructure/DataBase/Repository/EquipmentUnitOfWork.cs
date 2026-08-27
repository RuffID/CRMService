using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class EquipmentUnitOfWork(
        IUnitOfWorkScope scope,
        IEquipmentRepository equipment,
        IParameterRepository parameter,
        IKindRepository kind,
        IKindParamsRepository kindParams,
        IKindParameterRepository kindParameter,
        IMaintenanceEntityRepository maintenanceEntity,
        IManufacturerRepository manufacturer,
        IModelRepository model,
        ICompanyRepository company) : MainScenarioUnitOfWork(scope), IEquipmentUnitOfWork
    {
        public IEquipmentRepository Equipment { get; } = equipment;
        public IParameterRepository Parameter { get; } = parameter;
        public IKindRepository Kind { get; } = kind;
        public IKindParamsRepository KindParams { get; } = kindParams;
        public IKindParameterRepository KindParameter { get; } = kindParameter;
        public IMaintenanceEntityRepository MaintenanceEntity { get; } = maintenanceEntity;
        public IManufacturerRepository Manufacturer { get; } = manufacturer;
        public IModelRepository Model { get; } = model;
        public ICompanyRepository Company { get; } = company;
    }
}
