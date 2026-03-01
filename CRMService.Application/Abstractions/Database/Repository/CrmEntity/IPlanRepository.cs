using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanRepository : IGetItemByIdRepository<Plan, Guid>, IGetItemByPredicateRepository<Plan>, ICreateItemRepository<Plan>, IDeleteItemRepository<Plan>
    {
    }
}


