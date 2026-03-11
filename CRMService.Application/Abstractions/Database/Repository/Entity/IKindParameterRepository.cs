using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IKindParameterRepository :
        IGetItemByIdRepository<KindsParameter, int, DbContext>,
        IGetItemByPredicateRepository<KindsParameter, DbContext>,
        ICreateItemRepository<KindsParameter, DbContext>
    {
    }
}