namespace CRMService.Models.Dto.Authorization
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public bool Active { get; set; }

        public ICollection<CrmRoleDto> Roles { get; set; } = new List<CrmRoleDto>();
    }
}