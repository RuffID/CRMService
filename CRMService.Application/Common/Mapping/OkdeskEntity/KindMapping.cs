using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class KindMapping
    {
        public static IEnumerable<KindDto> ToDto(this IEnumerable<Kind> kinds)
        {
            foreach (Kind kind in kinds)
                yield return kind.ToDto();
        }

        public static KindDto ToDto(this Kind kind)
        {
            return new KindDto()
            {
                Id = kind.Id,
                Name = kind.Name,
                Code = kind.Code
            };
        }
    }
}



