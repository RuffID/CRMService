using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Domain.Models.Authorization
{
    public class Session : IEntity<Guid>, ICopyable<Session>
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpirationRefreshToken { get; set; }

        public virtual User User { get; set; } = null!;

        public void CopyData(Session newItem)
        {
            RefreshToken = newItem.RefreshToken;
            ExpirationRefreshToken = newItem.ExpirationRefreshToken;
        }
    }
}


