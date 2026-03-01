using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IManufacturerRepository : IGetItemByIdRepository<Manufacturer, int>, IGetItemByPredicateRepository<Manufacturer>,  ICreateItemRepository<Manufacturer>
    {
    }
}


