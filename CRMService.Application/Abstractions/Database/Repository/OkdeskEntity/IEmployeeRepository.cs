using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEmployeeRepository :
        IGetItemByIdRepository<Employee, int, DbContext>,
        IGetItemByPredicateRepository<Employee, DbContext>,
        ICreateItemRepository<Employee, DbContext>
    {
    }
}