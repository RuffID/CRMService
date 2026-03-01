using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.CrmEntities;

namespace CRMService.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanSettingRepository : IGetItemByPredicateRepository<PlanSetting>, ICreateItemRepository<PlanSetting>, IDeleteItemRepository<PlanSetting>
    {
    }
}