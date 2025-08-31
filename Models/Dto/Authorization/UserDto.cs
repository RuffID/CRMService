using CRMService.Interfaces.Entity;
using CRMService.Models.Authorization;

namespace CRMService.Models.Dto.Authorization
{
    public class UserDto : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool Active { get; set; }

        public ICollection<CrmRole> Roles { get; set; } = new List<CrmRole>();
    }
}