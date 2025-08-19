using CRMService.Dto.Authorization;

namespace CRMService.Models.Request
{
    public class UserRequestDto
    {
        public string? Login { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool? Active { get; set; }

        public ICollection<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}
