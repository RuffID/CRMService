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
        private readonly Hasher hasher = new(hashSettings);
        private readonly ILogger<UserRegistrationService> _logger = logger.CreateLogger<UserRegistrationService>();

        public async Task<bool> RegistrationUser(UserDto user)
        {
            // Поиск пользователя
            if (await unitOfWork.User.GetItem(mapper.Map<User>(user)) != null)
            {
                _logger.LogWarning("[Method:{MethodName}] User with login {userLogin} or email {email} already exists.", nameof(RegistrationUser), user.Login, user.Email);
                return false;
            }

            // Поиск ролей и присвоение найденных ролей пользователю
            user.Roles = await GetRoles(mapper.Map<User>(user));
            if (user.Roles == null || user.Roles.Count == 0)
            {
                _logger.LogWarning("[Method:{MethodName}] The roles specified in the request body were not found in the system.", nameof(RegistrationUser));
                return false;
            }

            // Хеширование пароля
            user.PasswordHash = hasher.Hash(user.PasswordHash!);
            // Создание пользователя, только после того как найдены роли, иначе нет смысла создавать без ролей
            user.Id = Guid.NewGuid();
            user.Active = true;
            unitOfWork.User.Create(mapper.Map<User>(user));
            await unitOfWork.SaveAsync();

            // Получение актуального id пользователя
            User? userInDb = await unitOfWork.User.GetItem(mapper.Map<User>(user));
            if (userInDb == null)
            {
                _logger.LogWarning("[Method:{MethodName}] Internal server error while retrieving user.", nameof(RegistrationUser));
                return false;
            }

            // Создание связей в таблице user_roles между пользователем и рол(ью/ями)
            if (!await CreateUserRoleConnect(userInDb))
            {
                _logger.LogWarning("[Method:{MethodName}] An internal error occurred while creating the user-role association.", nameof(RegistrationUser));
                return false;
            }

            _logger.LogInformation("[Method:{MethodName}] User: {userLogin} successfully created.", nameof(RegistrationUser), user.Login);
            return true;
        }

        private async Task<ICollection<Role>> GetRoles(User user)
        {
            // Поиск ролей
            ICollection<Role> roles = [];

            foreach (Role role in user.Roles)
            {
                // Получение id роли, если она есть в системе
                var item = await unitOfWork.Role.GetItem(role);
                if (item != null)
                    roles.Add(item);
            }
            return roles;
        }

        private async Task<bool> CreateUserRoleConnect(User userInDb)
        {
            // Создание связей в таблице user_roles между пользователем и рол(ью/ями)
            foreach (Role role in userInDb.Roles)
            {
                unitOfWork.UserRole.Create(new UserRole(Guid.NewGuid(), userInDb.Id, role.Id));
                await unitOfWork.SaveAsync();
            }
            return true;
        }
    }
}
