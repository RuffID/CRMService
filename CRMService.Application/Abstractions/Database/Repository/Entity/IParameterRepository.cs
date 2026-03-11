using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IParameterRepository :
        IGetItemByPredicateRepository<EquipmentParameter, DbContext>,
        ICreateItemRepository<EquipmentParameter, DbContext>
    {
    }
}