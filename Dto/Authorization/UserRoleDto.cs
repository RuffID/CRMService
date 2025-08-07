using CRMService.Interfaces.Authorization;

namespace CRMService.Dto.Authorization
{
    public class UserRoleDto : IEntity
    {
        public UserRoleDto() { }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }
    }
}