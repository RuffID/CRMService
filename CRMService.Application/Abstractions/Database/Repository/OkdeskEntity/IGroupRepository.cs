using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IGroupRepository : IGetItemByIdRepository<Group, int>, IGetItemByPredicateRepository<Group>, ICreateItemRepository<Group>
    {
    }
}


