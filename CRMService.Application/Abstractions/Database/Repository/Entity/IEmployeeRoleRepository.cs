using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEmployeeRoleRepository :
        IGetItemByPredicateRepository<EmployeeRole, DbContext>,
        ICreateItemRepository<EmployeeRole, DbContext>,
        IDeleteItemRepository<EmployeeRole, DbContext>
    {
    }
}