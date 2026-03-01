using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class GroupMapping
    {
        public static IEnumerable<GroupDto> ToDto(this IEnumerable<Group> groups)
        {
            foreach (Group group in groups)
                yield return group.ToDto();
        }

        public static GroupDto ToDto(this Group group)
        {
            return new GroupDto()
            {
                Id = group.Id,
                Name = group.Name,
            };
        }
    }
}



