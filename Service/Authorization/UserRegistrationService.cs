using AutoMapper;
using CRMService.Dto.Authorization;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Authorization
{
    public class UserRegistrationService(IUnitOfWorkAuthorization unitOfWork, IMapper mapper, ILoggerFactory logger, IOptions<HashSettings> hashSettings)
    {
        private readonly Hasher _hasher = new(hashSettings);
        private readonly ILogger<UserRegistrationService> _logger = logger.CreateLogger<UserRegistrationService>();

        public async Task<bool> RegistrationUser(UserDto userDto)
        {
            // Поиск пользователя
            if (await unitOfWork.User.GetItem(mapper.Map<User>(userDto), false) != null)
            {
                _logger.LogWarning("[Method:{MethodName}] User with login {userLogin} or email {email} already exists.", nameof(RegistrationUser), userDto.Login, userDto.Email);
                return false;
            }

            // Поиск ролей и присвоение найденных ролей пользователю
            ICollection<Role> roles = await unitOfWork.Role.GetItems(mapper.Map<User>(userDto).Roles);
            if (roles == null || roles.Count == 0)
            {
                _logger.LogWarning("[Method:{MethodName}] The roles specified in the request body were not found in the system. Login: {userLogin}.", nameof(RegistrationUser), userDto.Login);
                return false;
            }

            User user = new()
            {
                Login = userDto.Login,
                Email = userDto.Email,
                Roles = roles,
                PasswordHash = _hasher.Hash(userDto.PasswordHash!),
                Active = true
            };

            unitOfWork.User.Create(user);
            await unitOfWork.SaveAsync();

            _logger.LogInformation("[Method:{MethodName}] User: {userLogin} successfully created.", nameof(RegistrationUser), userDto.Login);
            return true;
        }
    }
}
