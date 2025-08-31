using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Authorization
{
    public class UserLoginService(IUnitOfWorkAuthorization unitOfWork, ILoggerFactory logger, IOptions<AuthorizationOptions> authOptions, GenerateRefreshToken generateRefresh)
    {
        private readonly JwtTokenService _accessToken = new(authOptions);        
        private readonly ILogger<UserLoginService> _logger = logger.CreateLogger<UserLoginService>();

        public async Task<Token> LoginInService(User user, CancellationToken ct)
        {
            Token token = new()
            {
                AccessToken = _accessToken.Create(user),
                RefreshToken = generateRefresh.Generate()
            };

            Session session = new()
            {
                UserId = user.Id,
                RefreshToken = token.RefreshToken,
                ExpirationRefreshToken = DateTime.UtcNow.AddDays(authOptions.Value.RefreshTokenLifeTimeFromDays)
            };

            unitOfWork.Session.Create(session);
            await unitOfWork.SaveAsync();

            return token;
        }

        public async Task<Token> UpdateTokens(Session session, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemById(session.UserId, asNoTracking: true, ct);

            if (user == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error, user {userId} not found.", nameof(UpdateTokens), session.UserId);
                throw new KeyNotFoundException($"User {session.UserId} not found.");
            }

            Token token = new()
            {
                AccessToken = _accessToken.Create(user),
                RefreshToken = generateRefresh.Generate()
            };

            session.RefreshToken = token.RefreshToken;
            session.ExpirationRefreshToken = DateTime.UtcNow.AddDays(authOptions.Value.RefreshTokenLifeTimeFromDays);

            await unitOfWork.SaveAsync();

            return token;
        }
    }
}