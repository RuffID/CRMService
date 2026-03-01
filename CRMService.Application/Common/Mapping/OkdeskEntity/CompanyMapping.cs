using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class CompanyMapping
    {
        public static IEnumerable<CompanyDto> ToDto(this IEnumerable<Company> companies)
        {
            foreach (Company company in companies)
                yield return company.ToDto();
        }

        public static CompanyDto ToDto(this Company company)
        {
            return new CompanyDto()
            {
                Id = company.Id,
                Name = company.Name,
                AdditionalName = company.AdditionalName,
                Active = company.Active
            };
        }
    }
}



