using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IModelRepository :
        IGetItemByIdRepository<Model, int, DbContext>,
        IGetItemByPredicateRepository<Model, DbContext>,
        ICreateItemRepository<Model, DbContext>
    {
    }
}