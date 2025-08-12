using AutoMapper;
using CRMService.Dto.Authorization;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class UserController(IOptions<HashSettings> hashSettings, IUnitOfWorkAuthorization unitOfWork, IMapper mapper, GenerateRandomString generateRandom) : Controller
    {
        private readonly HashVerify _hashVerify = new (hashSettings);
        private readonly Hasher _hasher = new (hashSettings);

        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUsers([FromQuery] int startIndex = 0, [FromQuery] int endIndex = 100)
        {
            IEnumerable<UserDto>? users = mapper.Map<IEnumerable<UserDto>>(await unitOfWork.User.GetAllItem(new Range(startIndex, endIndex)));

            if (!users.Any())
                return NotFound("Users not found.");

            return Ok(users);
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUser([FromBody] UserDto user)
        {
            var _user = mapper.Map<UserDto>(await unitOfWork.User.GetItem(mapper.Map<User>(user)));

            if (_user == null)
                return NotFound("User not found.");

            return Ok(_user);
        }

        [HttpPut("update_user"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto user)
        {
            // Поиск пользователя по логину
            User? userFromDb = await unitOfWork.User.GetItem(mapper.Map<User>(user));

            if (userFromDb == null)
                return NotFound($"User not found.");

            // Обновление пароля пользователя
            if (!string.IsNullOrEmpty(userFromDb.PasswordHash) && !string.IsNullOrEmpty(user.Password))
                user.Password = _hasher.Hash(user.Password);
            
            if (!string.IsNullOrEmpty(user.Email))
                userFromDb.Email = user.Email;

            if (user.Active != null)
                userFromDb.Active = user.Active;

            if (user.Roles != null && user.Roles.Count != 0)
                userFromDb.Roles = user.Roles;

            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("change_password")]
        public async Task<IActionResult> ChangeUserPassword([FromQuery] string login, [FromQuery] string oldPassword, [FromQuery] string newPassword)
        {
            // Поиск пользователя по логину
            User? user = await unitOfWork.User.GetItem(new User(login: login));

            if (user == null)
                return NotFound($"User not found.");

            // Сравнение хеша текущего пароля введённого пользователем с текущим пролем из БД
            if (string.IsNullOrEmpty(user.PasswordHash) || !_hashVerify.Verify(oldPassword, user.PasswordHash))
                return Unauthorized("Password is incorrect.");

            // Обновление пароля пользователя
            user.PasswordHash = _hasher.Hash(newPassword);
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("update_email"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> UpdateEmail([FromBody] UserDto updateUser)
        {
            User? user = await unitOfWork.User.GetItem(mapper.Map<User>(updateUser));

            if (user == null)
                return NotFound($"User by login {updateUser.Login} - not found.");

            user.Email = updateUser.Email;
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("restore_password"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> RestoreUserPassword([FromQuery] string login)
        {
            User? user = await unitOfWork.User.GetItem(new User(login: login));

            if (user == null)
                return NotFound($"User by login {login} - not found.");

            string newPassword = generateRandom.GetString(length: 12);

            user.PasswordHash = _hasher.Hash(newPassword);      

            await unitOfWork.SaveAsync();

            return Ok("Password updated");
        }
    }
}