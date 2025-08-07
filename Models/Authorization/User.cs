using CRMService.Interfaces.Authorization;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.Authorization
{
    public class User : IEntity
    {
        public User() { }

        public User(string? login = null, string? password = null, string? email = null) 
        {
            Login = login;
            PasswordHash = password;
            Email = email;
        }

        public Guid Id { get; set; }

        public string? Login { get; set; }

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public bool? Active { get; set; }

        [NotMapped]
        public ICollection<Role> Roles { get; set; } = [];

        public virtual BlockReason? BlockReason { get; set; }

        public virtual ICollection<Session> Sessions { get; set; } = [];
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = [];        
    }
}