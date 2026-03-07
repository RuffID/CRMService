using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanRepository :
        IGetItemByIdRepository<Plan, Guid, DbContext>,
        IGetItemByPredicateRepository<Plan, DbContext>,
        ICreateItemRepository<Plan, DbContext>,
        IDeleteItemRepository<Plan, DbContext>
    {
    }
}