namespace CRMService.Contracts.Models.Dto.Authorization
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public bool Active { get; set; }

        public ICollection<CrmRoleDto> Roles { get; set; } = new List<CrmRoleDto>();
    }
}