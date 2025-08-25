using CRMService.Interfaces.Authorization;

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

        public virtual BlockReason? BlockReason { get; set; }

        public virtual ICollection<CrmRole> Roles { get; set; } = new List<CrmRole>();

        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public void CopyData(User newItem)
        {
            Login = newItem.Login;
            Email = newItem.Email;
            PasswordHash = newItem.PasswordHash;
            Active = newItem.Active;
        }
    }
}