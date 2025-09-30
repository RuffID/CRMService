using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;

namespace CRMService.Models.Dto.Mappers.Authorize
{
    public static class SessionMapping
    {
        public static IEnumerable<SessionDto> ToDto(this IEnumerable<Session> sessions)
        {
            foreach (Session session in sessions)
                yield return session.ToDto();
        }

        public static SessionDto ToDto(this Session session)
        {
            return new SessionDto()
            {
                Id = session.Id,
                UserId = session.UserId,
                RefreshToken = session.RefreshToken,
                ExpirationRefreshToken = session.ExpirationRefreshToken
            };
        }
    }
}
