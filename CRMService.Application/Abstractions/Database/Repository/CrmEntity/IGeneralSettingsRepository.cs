using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IGeneralSettingsRepository :
        IGetItemByIdRepository<GeneralSettings, Guid, DbContext>,
        IGetItemByPredicateRepository<GeneralSettings, DbContext>,
        ICreateItemRepository<GeneralSettings, DbContext>,
        IDeleteItemRepository<GeneralSettings, DbContext>
    {
    }
}