using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IManufacturerRepository : IGetItemByIdRepository<Manufacturer, int>, IGetItemByPredicateRepository<Manufacturer>, IUpsertItemByIdRepository<Manufacturer, int>, ICreateItemRepository<Manufacturer>
    {
    }
}