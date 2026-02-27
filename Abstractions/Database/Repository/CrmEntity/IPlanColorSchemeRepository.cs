using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.CrmEntities;

namespace CRMService.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanColorSchemeRepository : IGetItemByIdRepository<PlanColorScheme, Guid>, IGetItemByPredicateRepository<PlanColorScheme>, ICreateItemRepository<PlanColorScheme>, IDeleteItemRepository<PlanColorScheme>
    {
    }
}
