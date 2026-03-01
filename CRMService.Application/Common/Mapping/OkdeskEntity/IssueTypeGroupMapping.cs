using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class IssueTypeGroupMapping
    {
        public static IEnumerable<IssueTypeGroupDto> ToDto(this IEnumerable<IssueTypeGroup> types)
        {
            foreach (IssueTypeGroup type in types)
                yield return type.ToDto();
        }

        public static IssueTypeGroupDto ToDto(this IssueTypeGroup type)
        {
            return new IssueTypeGroupDto()
            {
                Id = type.Id,
                Name = type.Name,
                Code = type.Code,
                ParentId = type.ParentGroupId
            };
        }
    }
}



