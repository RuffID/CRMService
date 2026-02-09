using CRMService.Abstractions.Entity;

namespace CRMService.Models.Authorization
{
    public class CrmRole : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; } = new List<User>();

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}