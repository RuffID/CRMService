using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Domain.Models.Authorization;
using CRMService.Domain.Models.Constants;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.Authorization
{
    public class AuthenticationService(
        IAuthorizationUnitOfWork unitOfWork,
        Hasher hasher,
        IAccessTokenService accessTokenService,
        IRandomStringGenerator randomStringGenerator,
        ILogger<AuthenticationService> logger)
    {
        public async Task<User?> AuthenticateAsync(string login, string password, bool ignoreLoginCase, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetByLoginWithRolesReadOnlyAsync(login, ignoreLoginCase, ct);

            if (user == null || !user.Active || string.IsNullOrEmpty(user.Password) || !hasher.Verify(password, user.Password))
                return null;

            return user;
        }

        public Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken ct) =>
            unitOfWork.User.GetByIdWithRolesReadOnlyAsync(userId, ct);

        public async Task<Token?> LoginAsync(string login, string password, CancellationToken ct)
        {
            User? user = await AuthenticateAsync(login, password, ignoreLoginCase: false, ct);
            if (user == null)
                return null;

            Token token = CreateToken(user);
            Session session = new()
            {
                UserId = user.Id,
                RefreshToken = token.RefreshToken,
                ExpirationRefreshToken = DateTime.UtcNow.AddDays(JWTSettingsConstants.REFRESH_TOKEN_LIFE_TIME_FROM_DAYS)
            };

            unitOfWork.Session.Create(session);
            await unitOfWork.SaveChangesAsync(ct);
            return token;
        }

        public async Task<Token?> UpdateTokensAsync(string refreshToken, CancellationToken ct)
        {
            Session? session = await unitOfWork.Session.GetByRefreshTokenAsync(refreshToken, ct);

            if (session == null)
                return null;

            User? user = await unitOfWork.User.GetItemByIdReadOnlyAsync(session.UserId, ct);
            if (user == null)
            {
                logger.LogError("[Method:{MethodName}] Internal server error, user {UserId} not found.", nameof(UpdateTokensAsync), session.UserId);
                return null;
            }

            Token token = CreateToken(user);
            session.RefreshToken = token.RefreshToken;
            session.ExpirationRefreshToken = DateTime.UtcNow.AddDays(JWTSettingsConstants.REFRESH_TOKEN_LIFE_TIME_FROM_DAYS);
            await unitOfWork.SaveChangesAsync(ct);
            return token;
        }

        private Token CreateToken(User user) => new()
        {
            AccessToken = accessTokenService.Create(user),
            RefreshToken = randomStringGenerator.GetBase64RandomString()
        };
    }
}
