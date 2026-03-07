using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IManufacturerRepository :
        IGetItemByIdRepository<Manufacturer, int, DbContext>,
        IGetItemByPredicateRepository<Manufacturer, DbContext>,
        ICreateItemRepository<Manufacturer, DbContext>
    {
    }
}