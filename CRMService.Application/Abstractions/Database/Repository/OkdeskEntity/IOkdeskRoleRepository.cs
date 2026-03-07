using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskRoleRepository :
        IGetItemByIdRepository<OkdeskRole, int, DbContext>,
        IGetItemByPredicateRepository<OkdeskRole, DbContext>,
        ICreateItemRepository<OkdeskRole, DbContext>
    {
    }
}