using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IManufacturerRepository : IGetItemByIdRepository<Manufacturer, int>, IGetItemByPredicateRepository<Manufacturer>, IUpsertItemByIdRepository<Manufacturer, int>, ICreateItemRepository<Manufacturer>
    {
    }
}