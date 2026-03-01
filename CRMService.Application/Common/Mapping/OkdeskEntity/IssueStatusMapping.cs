using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class IssueStatusMapping
    {
        public static IEnumerable<StatusDto> ToDto(this IEnumerable<IssueStatus> statuses)
        {
            foreach (IssueStatus status in statuses)
                yield return status.ToDto();
        }

        public static StatusDto ToDto(this IssueStatus status)
        {
            return new StatusDto()
            {
                Id = status.Id,
                Name = status.Name,
                Code = status.Code
            };
        }
    }
}



