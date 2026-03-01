using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.CrmEntities;

namespace CRMService.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanRepository : IGetItemByIdRepository<Plan, Guid>, IGetItemByPredicateRepository<Plan>, ICreateItemRepository<Plan>, IDeleteItemRepository<Plan>
    {
    }
}