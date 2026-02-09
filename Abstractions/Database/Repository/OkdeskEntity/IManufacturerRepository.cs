using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IManufacturerRepository : IGetItemByIdRepository<Manufacturer, int>, IGetItemByPredicateRepository<Manufacturer>,  ICreateItemRepository<Manufacturer>
    {
    }
}