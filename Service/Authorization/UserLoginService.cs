using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Authorization
{
    public class UserLoginService(IUnitOfWorkAuthorization unitOfWork, ILoggerFactory logger, IOptions<AuthOptions> authOptions, GenerateRefreshToken generateRefresh)
    {
        private readonly GenerateAccessToken _accessToken = new(authOptions);        
        private readonly ILogger<UserLoginService> _logger = logger.CreateLogger<UserLoginService>();

        public async Task<Token?> LoginInService(User user)
        {
            // Генерация токена
            Token? token = GetToken(user);
            if (token == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error while generating token.", nameof(LoginInService));
                return null;
            }

            // Создание сессии
            Session session = new();
            SetSession(session, token, user);

            // Сохранение сессии
            session.Id = Guid.NewGuid();
            unitOfWork.Session.Create(session);
            await unitOfWork.SaveAsync();

            return token;
        }

        public async Task<Token?> UpdateTokens(Session session)
        {
            User? user = await unitOfWork.User.GetUserWithRoles(new User() { Id = session.UserId }, false);

            if (user == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error, user not found.", nameof(UpdateTokens));
                return null;
            }

            Token? token = GetToken(user);
            if (token == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error while generating token.", nameof(UpdateTokens));
                return null;
            }

            Session? oldSession = await unitOfWork.Session.GetItem(session);
            if (oldSession != null)
                SetSession(session, token, user);            
            else            
                unitOfWork.Session.Create(session);

            await unitOfWork.SaveAsync();

            return token;
        }

        private Token? GetToken(User user)
        {
            // Генерация токена
            Token token = new()
            {
                // Срок действия access token указывается в конфиге
                AccessToken = _accessToken.Generate(user),
                RefreshToken = generateRefresh.Generate()
            };

            if (!string.IsNullOrEmpty(token.AccessToken) && !string.IsNullOrEmpty(token.RefreshToken))
                return token;

            return null;
        }

        private void SetSession(Session session, Token token, User user)
        {
            // Создание сессии
            session.UserId = user.Id;
            session.RefreshToken = token.RefreshToken;
            // Срок действия refresh token указывается в конфиге
            session.ExpirationRefreshToken = DateTime.UtcNow.AddDays(authOptions.Value.RefreshTokenLifeTimeFromDays);
        }
    }
}
