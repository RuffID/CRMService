using CRMService.Interfaces.Authorization;

namespace CRMService.Models.Authorization
{
    public class CrmRole : IEntity
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}