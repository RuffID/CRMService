using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IGroupRepository :
        IGetItemByIdRepository<Group, int, DbContext>,
        IGetItemByPredicateRepository<Group, DbContext>,
        ICreateItemRepository<Group, DbContext>
    {
    }
}