using CRMService.Interfaces.Entity;

namespace CRMService.Models.Dto.Authorization
{
    public class SessionDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? ExpirationRefreshToken { get; set; }
    }
}