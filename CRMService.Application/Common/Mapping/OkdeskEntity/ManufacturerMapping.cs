using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class ManufacturerMapping
    {
        public static IEnumerable<ManufacturerDto> ToDto(this IEnumerable<Manufacturer> manufacturers)
        {
            foreach (Manufacturer manufacturer in manufacturers)
                yield return manufacturer.ToDto();
        }

        public static ManufacturerDto ToDto(this Manufacturer manufacturer)
        {
            return new ManufacturerDto()
            {
                Id = manufacturer.Id,
                Name = manufacturer.Name,
                Code = manufacturer.Code
            };
        }
    }
}



