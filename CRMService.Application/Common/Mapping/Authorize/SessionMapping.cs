using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;

namespace CRMService.Application.Common.Mapping.Authorize
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



