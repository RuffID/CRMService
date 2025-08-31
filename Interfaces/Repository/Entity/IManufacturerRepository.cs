using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IManufacturerRepository : IGetItemByIdRepository<Manufacturer, int>, IGetItemByCodeRepository<Manufacturer>, IUpsertItemByCodeRepository<Manufacturer>, ICreateItemRepository<Manufacturer>
    {
    }
}