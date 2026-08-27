using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IGeneralSettingsRepository :
        IGetItemByIdRepository<GeneralSettings, Guid>,
        IGetItemsRepository<GeneralSettings>,
        ICreateItemRepository<GeneralSettings>,
        IDeleteItemRepository<GeneralSettings>
    {
    }
}
