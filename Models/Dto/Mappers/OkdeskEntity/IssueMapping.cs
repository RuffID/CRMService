using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class IssueMapping
    {
        public static IEnumerable<IssueDto> ToDto(this IEnumerable<Issue> issues)
        {
            foreach (Issue issue in issues)
                yield return issue.ToDto();
        }

        public static IssueDto ToDto(this Issue issue)
        {
            return new IssueDto()
            {
                Id = issue.Id,
                AssigneeId = issue.AssigneeId,
                AuthorId = issue.AuthorId,
                Title = issue.Title,
                EmployeesUpdatedAt = issue.EmployeesUpdatedAt,
                CreatedAt = issue.CreatedAt,
                CompletedAt = issue.CompletedAt,
                DeadlineAt = issue.DeadlineAt,
                DelayTo = issue.DelayTo,
                StatusId = issue.StatusId,
                PriorityId = issue.PriorityId,
                TypeId = issue.TypeId,
                CompanyId = issue.CompanyId,
                ServiceObjectId = issue.ServiceObjectId
            };
        }
    }
}
