using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Authorization
{
    public class UserLoginService(IUnitOfWorkAuthorization unitOfWork, ILoggerFactory logger, IOptions<AuthOptions> authOptions)
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
            // Поиск пользователя
            User? user = await unitOfWork.User.GetItem(new User() { Id = session.UserId });

            if (user == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error, user not found.", nameof(UpdateTokens));
                return null;
            }

            string? error = await GetRoles(user);
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("[Method:{MethodName}] {error}", nameof(LoginInService), error);
                return null;
            }

            // Генерация токена
            Token? token = GetToken(user);
            if (token == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error while generating token.", nameof(UpdateTokens));
                return null;
            }

            Session? oldSession = await unitOfWork.Session.GetItem(session);
            if (oldSession == null)
                unitOfWork.Session.Create(session);
            else
            {
                // Задать новые значения в сессию
                SetSession(session, token, user);
                unitOfWork.Session.Update(oldSession, session);
            }

            await unitOfWork.SaveAsync();

            return token;
        }

        private async Task<string?> GetRoles(User user)
        {
            //Получить ролей пользователя
            IEnumerable<Role>? roles = await unitOfWork.UserRole.GetRolesByUserId(user.Id);

            if (roles == null || !roles.Any())
                return "Internal server error while retrieving user roles.";

            // Т.к. у одного пользователя может быть несколько ролей - добавляет все найденные роли в список к пользователю
            foreach (var role in roles)
                user.Roles.Add(role);

            return null;
        }

        private Token? GetToken(User user)
        {
            // Генерация токена
            Token token = new()
            {
                // Срок действия access token указывается в конфиге
                AccessToken = _accessToken.Generate(user),
                RefreshToken = GenerateRefreshToken.Generate()
            };

            if (string.IsNullOrEmpty(token.AccessToken) || string.IsNullOrEmpty(token.RefreshToken))
                return null;
            else return token;
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
