using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanColorSchemeRepository : IGetItemByIdRepository<PlanColorScheme, Guid>, IGetItemByPredicateRepository<PlanColorScheme>, ICreateItemRepository<PlanColorScheme>, IDeleteItemRepository<PlanColorScheme>
    {
    }
}



