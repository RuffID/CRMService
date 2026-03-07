using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanSettingRepository :
        IGetItemByPredicateRepository<PlanSetting, DbContext>,
        ICreateItemRepository<PlanSetting, DbContext>,
        IDeleteItemRepository<PlanSetting, DbContext>
    {
    }
}