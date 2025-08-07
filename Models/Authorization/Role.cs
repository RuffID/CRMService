using CRMService.Interfaces.Authorization;

namespace CRMService.Models.Authorization
{
    public class Role : IEntity
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}