using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
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



