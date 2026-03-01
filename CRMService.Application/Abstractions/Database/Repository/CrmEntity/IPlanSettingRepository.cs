using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanSettingRepository : IGetItemByPredicateRepository<PlanSetting>, ICreateItemRepository<PlanSetting>, IDeleteItemRepository<PlanSetting>
    {
    }
}


