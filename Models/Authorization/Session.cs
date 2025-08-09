using CRMService.Interfaces.Authorization;

namespace CRMService.Models.Authorization
{
    public class Session : IEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? ExpirationRefreshToken { get; set; }

        public virtual User? User { get; set; }

        internal void CopyData(Session newItem)
        {
            throw new NotImplementedException();
        }
    }
}