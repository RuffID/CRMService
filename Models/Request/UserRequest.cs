using CRMService.Models.Dto.Authorization;

namespace CRMService.Models.Request
{
    public class UserRequest
    {
        public string Login { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool Active { get; set; }

        public ICollection<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}