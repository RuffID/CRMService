using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IGeneralSettingsRepository : IGetItemByIdRepository<GeneralSettings, Guid>, IGetItemByPredicateRepository<GeneralSettings>, ICreateItemRepository<GeneralSettings>, IDeleteItemRepository<GeneralSettings>
    {
    }
}


