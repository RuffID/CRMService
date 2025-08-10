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
    [Route("api/[controller]")]
    public class UserController(IOptions<HashSettings> hashSettings, IUnitOfWorkAuthorization unitOfWork, IMapper mapper) : Controller
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
            if (!string.IsNullOrEmpty(userFromDb.PasswordHash) && !string.IsNullOrEmpty(user.PasswordHash))
                user.PasswordHash = _hasher.Hash(user.PasswordHash);
            
            if (!string.IsNullOrEmpty(user.Email))
                userFromDb.Email = user.Email;

            if (user.Active != null)
                userFromDb.Active = user.Active;

            // TODO ещё нужно обновлять роли

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

            if (string.IsNullOrEmpty(user.PasswordHash))
                return StatusCode(500, "Internal server error.");

            // Сравнение хеша текущего пароля введённого пользователем с текущим пролем из БД
            if (!_hashVerify.Verify(oldPassword, user.PasswordHash))
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

            if (user == null || string.IsNullOrEmpty(updateUser.Email))
                return NotFound("User not found or email not specified.");

            user.Email = updateUser.Email;
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("restore_password"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> RestoreUserPassword([FromQuery] string login)
        {
            //TODO сначала отправлять код подтверждения, а только потом сбрасывать новый пароль

            User? user = await unitOfWork.User.GetItem(new User(login: login));

            if (user == null || string.IsNullOrEmpty(user.Login))
                return NotFound("User not found.");

            string newPassword = GenerateRandomString.GetString(length: 12);

            // Обновление пароля пользователя
            user.PasswordHash = _hasher.Hash(newPassword);
            
            //TODO починить почту
            // Отправка пароля на почту
            // Пока не работает
            /*MailMessage? mail = smtp.CreateMail(name: smtpSettings.Value.Name, emailFrom: smtpSettings.Value.Email, emailTo: user.Email, 
                subject: "Новый пароль от сервиса", body: $"Пароль: {newPassword}");

            if (!smtp.SendMail("smtp.gmail.com", 587, smtpSettings.Value.Email, smtpSettings.Value.Password, mail))
                return StatusCode(500, "Internal server error while sending password to email.");  */          

            await unitOfWork.SaveAsync();

            return Ok("New password: " + newPassword);
        }
    }
}