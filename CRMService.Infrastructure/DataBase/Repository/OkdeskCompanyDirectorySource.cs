using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class OkdeskCompanyDirectorySource(
        IOkdeskCompanyCategoryRepository companyCategory,
        IOkdeskCompanyRepository company,
        IOkdeskEmployeeRepository employee,
        IOkdeskGroupRepository group) : IOkdeskCompanyDirectorySource
    {
        public IOkdeskCompanyCategoryRepository CompanyCategory { get; } = companyCategory;
        public IOkdeskCompanyRepository Company { get; } = company;
        public IOkdeskEmployeeRepository Employee { get; } = employee;
        public IOkdeskGroupRepository Group { get; } = group;
    }
}
