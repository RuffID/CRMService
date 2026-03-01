using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.CrmEntities;

namespace CRMService.Abstractions.Database.Repository.CrmEntity
{
    public interface IGeneralSettingsRepository : IGetItemByIdRepository<GeneralSettings, Guid>, IGetItemByPredicateRepository<GeneralSettings>, ICreateItemRepository<GeneralSettings>, IDeleteItemRepository<GeneralSettings>
    {
    }
}