using CRMService.Interfaces.Authorization;

namespace CRMService.Dto.Authorization
{
    public class SessionDto : IEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? ExpirationRefreshToken { get; set; }
    }
}