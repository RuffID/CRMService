using CRMService.Abstractions.Entity;

namespace CRMService.Models.Authorization
{
    public class User : IEntity<Guid>, ICopyable<User>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool Active { get; set; }

        public virtual BlockReason? BlockReason { get; set; }

        public virtual ICollection<CrmRole> Roles { get; set; } = new List<CrmRole>();

        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public void CopyData(User newItem)
        {
            Login = newItem.Login;
            Password = newItem.Password;
            Active = newItem.Active;
        }
    }
}