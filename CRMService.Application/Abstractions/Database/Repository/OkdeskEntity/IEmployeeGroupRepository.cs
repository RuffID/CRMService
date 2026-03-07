using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEmployeeGroupRepository :
        IGetItemByPredicateRepository<EmployeeGroup, DbContext>,
        ICreateItemRepository<EmployeeGroup, DbContext>,
        IDeleteItemRepository<EmployeeGroup, DbContext>
    {
    }
}