using CRMService.Models.Dto.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CRMService.Models.Request
{
    public class UserRequest
    {
        [MinLength(1, ErrorMessage = "Login must not be empty")]
        public string Login { get; set; } = null!;

        [MinLength(1, ErrorMessage = "Login must not be empty")]
        public string Name { get; set; } = null!;

        [MinLength(1, ErrorMessage = "Password must not be empty")]
        public string Password { get; set; } = null!;

        [MinLength(1, ErrorMessage = "User role must not be empty.")]
        public ICollection<RoleDto> Roles { get; set; } = null!;
    }
}