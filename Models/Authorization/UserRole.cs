using CRMService.Interfaces.Authorization;

namespace CRMService.Models.Authorization
{
    public class UserRole : IEntity
    {
        public UserRole() { }

        public UserRole(Guid id, Guid userId, Guid roleId) 
        {
            Id = id;
            UserId = userId;
            RoleId = roleId;
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public virtual Role? Role { get; set; }

        public virtual User? User { get; set; }
    }
}