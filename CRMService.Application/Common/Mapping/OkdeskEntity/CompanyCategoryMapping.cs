using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class CompanyCategoryMapping
    {
        public static IEnumerable<CompanyCategoryDto> ToDto(this IEnumerable<CompanyCategory> categories)
        {
            foreach (CompanyCategory category in categories)
                yield return category.ToDto();
        }

        public static CompanyCategoryDto ToDto(this CompanyCategory category)
        {
            return new CompanyCategoryDto()
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Color = category.Color
            };
        }

        public static CompanyCategory ToEntity(this CompanyCategoryDto categoryDto)
        {
            return new CompanyCategory()
            {
                Id = categoryDto.Id,
                Name = categoryDto.Name,
                Code = categoryDto.Code,
                Color = categoryDto.Color
            };
        }
    }
}


