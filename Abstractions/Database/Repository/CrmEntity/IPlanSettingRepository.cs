using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.CrmEntities;

namespace CRMService.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanSettingRepository : IGetItemByIdRepository<PlanSetting, Guid>, IGetItemByPredicateRepository<PlanSetting>, ICreateItemRepository<PlanSetting>, IDeleteItemRepository<PlanSetting>
    {
    }
}
