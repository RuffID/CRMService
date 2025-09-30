using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class KindsParameterMapping
    {
        public static IEnumerable<KindParameterDto> ToDto(this IEnumerable<KindsParameter> parameters)
        {
            foreach (KindsParameter parameter in parameters)
                yield return parameter.ToDto();
        }

        public static KindParameterDto ToDto(this KindsParameter parameter)
        {
            return new KindParameterDto()
            {
                Id = parameter.Id,
                Name = parameter.Name
            };
        }
    }
}
