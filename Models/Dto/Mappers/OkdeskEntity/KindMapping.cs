using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
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
