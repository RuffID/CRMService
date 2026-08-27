using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface ICompanyDirectoryUnitOfWork : IUnitOfWorkScope
    {
        ICompanyRepository Company { get; }
        ICompanyCategoryRepository CompanyCategory { get; }
        IEmployeeRepository Employee { get; }
        IEmployeeGroupRepository EmployeeGroup { get; }
        IEmployeeRoleRepository EmployeeRole { get; }
        IGroupRepository Group { get; }
        IOkdeskRoleRepository OkdeskRole { get; }
    }
}
