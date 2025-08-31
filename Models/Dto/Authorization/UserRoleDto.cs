namespace CRMService.Models.Dto.Authorization
{
    public class UserRoleDto
    {
        public UserRoleDto() { }

        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }
    }
}