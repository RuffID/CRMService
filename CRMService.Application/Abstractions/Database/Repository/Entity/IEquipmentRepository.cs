using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEquipmentRepository :
        IGetItemByIdRepository<Equipment, int, DbContext>,
        IGetItemByPredicateRepository<Equipment, DbContext>,
        ICreateItemRepository<Equipment, DbContext>
    {
    }
}