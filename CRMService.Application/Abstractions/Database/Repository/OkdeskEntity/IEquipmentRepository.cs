using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEquipmentRepository : IGetItemByIdRepository<Equipment, int>, IGetItemByPredicateRepository<Equipment>, ICreateItemRepository<Equipment>
    {
    }
}


