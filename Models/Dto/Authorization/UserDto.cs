using CRMService.Interfaces.Authorization;
using CRMService.Models.Authorization;

namespace CRMService.Models.Dto.Authorization
{
    public class UserDto : IEntity
    {
        public Guid Id { get; set; }

        public string? Login { get; set; }

        public string? Email { get; set; }

        public bool? Active { get; set; }

        public ICollection<CrmRole> Roles { get; set; } = new List<CrmRole>();
    }
}