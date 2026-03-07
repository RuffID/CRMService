using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskEmployeeRepository :
        IGetItemByIdRepository<Employee, int, DbContext>,
        IGetItemByPredicateRepository<Employee, DbContext>
    {
    }
}
