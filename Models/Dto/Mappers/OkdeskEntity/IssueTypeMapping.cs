using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class IssueTypeMapping
    {
        public static IEnumerable<TaskTypeDto> ToDto(this IEnumerable<IssueType> types)
        {
            foreach (IssueType type in types)
                yield return type.ToDto();
        }

        public static TaskTypeDto ToDto(this IssueType type)
        {
            return new TaskTypeDto()
            {
                Id = type.Id,
                Name = type.Name,
                Code = type.Code,
                GroupId = type.GroupId,
                GroupName = type.Group?.Name
            };
        }
    }
}
