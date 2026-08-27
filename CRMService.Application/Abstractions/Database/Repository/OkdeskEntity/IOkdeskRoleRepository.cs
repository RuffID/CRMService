using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskRoleRepository :
        IGetItemByIdRepository<OkdeskRole, int>,
        IGetItemsRepository<OkdeskRole>,
        ICreateItemRepository<OkdeskRole>
    {
    }
}
