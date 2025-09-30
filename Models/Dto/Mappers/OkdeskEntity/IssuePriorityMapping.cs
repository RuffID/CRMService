using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class IssuePriorityMapping
    {
        public static IEnumerable<PriorityDto> ToDto(this IEnumerable<IssuePriority> priorities)
        {
            foreach (IssuePriority priority in priorities)
                yield return priority.ToDto();
        }

        public static PriorityDto ToDto(this IssuePriority priority)
        {
            return new PriorityDto()
            {
                Id = priority.Id,
                Name = priority.Name,
                Code = priority.Code
            };
        }
    }
}
