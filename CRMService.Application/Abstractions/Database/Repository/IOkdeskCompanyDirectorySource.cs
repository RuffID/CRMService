using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IOkdeskCompanyDirectorySource
    {
        IOkdeskCompanyCategoryRepository CompanyCategory { get; }
        IOkdeskCompanyRepository Company { get; }
        IOkdeskEmployeeRepository Employee { get; }
        IOkdeskGroupRepository Group { get; }
    }
}
