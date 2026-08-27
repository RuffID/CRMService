using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class CompanyDirectoryUnitOfWork(
        IUnitOfWorkScope scope,
        ICompanyRepository company,
        ICompanyCategoryRepository companyCategory,
        IEmployeeRepository employee,
        IEmployeeGroupRepository employeeGroup,
        IEmployeeRoleRepository employeeRole,
        IGroupRepository group,
        IOkdeskRoleRepository okdeskRole) : MainScenarioUnitOfWork(scope), ICompanyDirectoryUnitOfWork
    {
        public ICompanyRepository Company { get; } = company;
        public ICompanyCategoryRepository CompanyCategory { get; } = companyCategory;
        public IEmployeeRepository Employee { get; } = employee;
        public IEmployeeGroupRepository EmployeeGroup { get; } = employeeGroup;
        public IEmployeeRoleRepository EmployeeRole { get; } = employeeRole;
        public IGroupRepository Group { get; } = group;
        public IOkdeskRoleRepository OkdeskRole { get; } = okdeskRole;
    }
}
