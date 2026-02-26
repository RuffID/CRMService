using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Dto.Mappers.Authorize;
using CRMService.Models.Responses.Results;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Service.Authorization
{
    public class UserService(IUnitOfWork unitOfWork, Hasher hasher)
    {
        public async Task<ServiceResult<List<UserDto>>> GetUsersAsync(CancellationToken ct)
        {
            List<User> users = await unitOfWork.User.GetItemsByPredicateAsync(
                asNoTracking: true,
                include: q => q.Include(u => u.Roles),
                ct: ct);

            List<UserDto> data = users
                .OrderBy(u => u.Login)
                .ToDto()
                .ToList();

            return ServiceResult<List<UserDto>>.Ok(data);
        }

        public async Task<ServiceResult> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
        {
            string name = (request.Name ?? string.Empty).Trim();
            string login = (request.Login ?? string.Empty).Trim();
            string password = request.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
                return ServiceResult.Fail(400, "Имя обязательно.");

            if (string.IsNullOrWhiteSpace(login))
                return ServiceResult.Fail(400, "Логин обязателен.");

            if (string.IsNullOrWhiteSpace(password))
                return ServiceResult.Fail(400, "Пароль обязателен.");

            if (name.Length > 100)
                return ServiceResult.Fail(400, "Имя не должно быть длиннее 100 символов.");

            if (login.Length > 45)
                return ServiceResult.Fail(400, "Логин не должен быть длиннее 45 символов.");

            User? existingUser = await unitOfWork.User.GetItemByPredicateAsync(
                u => u.Login.ToLower() == login.ToLower(),
                asNoTracking: true,
                ct: ct);

            if (existingUser != null)
                return ServiceResult.Fail(409, $"Пользователь с логином '{login}' уже существует.");

            List<Guid> roleIds = (request.RoleIds ?? []).Distinct().ToList();
            List<CrmRole> roles = roleIds.Count == 0
                ? []
                : await unitOfWork.CrmRole.GetItemsByPredicateAsync(r => roleIds.Contains(r.Id), asNoTracking: false, ct: ct);

            User user = new()
            {
                Name = name,
                Login = login,
                Password = hasher.Hash(password),
                Active = true,
                Roles = roles
            };

            unitOfWork.User.Create(user);
            await unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateUserAsync(UpdateUserRequest request, CancellationToken ct)
        {
            if (request.UserId == Guid.Empty)
                return ServiceResult.Fail(400, "Не указан пользователь.");

            string name = (request.Name ?? string.Empty).Trim();
            string login = (request.Login ?? string.Empty).Trim();
            string password = request.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
                return ServiceResult.Fail(400, "Имя обязательно.");

            if (string.IsNullOrWhiteSpace(login))
                return ServiceResult.Fail(400, "Логин обязателен.");

            if (name.Length > 100)
                return ServiceResult.Fail(400, "Имя не должно быть длиннее 100 символов.");

            if (login.Length > 45)
                return ServiceResult.Fail(400, "Логин не должен быть длиннее 45 символов.");

            User? user = await unitOfWork.User.GetItemByIdAsync(
                request.UserId,
                asNoTracking: false,
                include: q => q.Include(u => u.Roles),
                ct: ct);

            if (user == null)
                return ServiceResult.Fail(404, "Пользователь не найден.");

            User? sameLogin = await unitOfWork.User.GetItemByPredicateAsync(
                u => u.Id != request.UserId && u.Login.ToLower() == login.ToLower(),
                asNoTracking: true,
                ct: ct);

            if (sameLogin != null)
                return ServiceResult.Fail(409, $"Пользователь с логином '{login}' уже существует.");

            user.Name = name;
            user.Login = login;

            if (!string.IsNullOrWhiteSpace(password))
                user.Password = hasher.Hash(password);

            List<Guid> roleIds = (request.RoleIds ?? []).Distinct().ToList();
            List<CrmRole> roles = roleIds.Count == 0
                ? []
                : await unitOfWork.CrmRole.GetItemsByPredicateAsync(r => roleIds.Contains(r.Id), asNoTracking: false, ct: ct);

            user.Roles.Clear();
            foreach (CrmRole role in roles)
                user.Roles.Add(role);

            await unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeactivateUserAsync(Guid currentUserId, Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty)
                return ServiceResult.Fail(400, "Не указан пользователь.");

            if (currentUserId == userId)
                return ServiceResult.Fail(400, "Нельзя деактивировать текущего пользователя.");

            User? user = await unitOfWork.User.GetItemByIdAsync(userId, asNoTracking: false, ct: ct);
            if (user == null)
                return ServiceResult.Fail(404, "Пользователь не найден.");

            if (!user.Active)
                return ServiceResult.Ok();

            user.Active = false;
            await unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok();
        }

        public bool IsAdmin(User user)
        {
            return user.Roles.Any(r => string.Equals(r.Name, RolesConstants.ADMIN, StringComparison.OrdinalIgnoreCase));
        }

        public sealed class CreateUserRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public List<Guid>? RoleIds { get; set; }
        }

        public sealed class UpdateUserRequest
        {
            public Guid UserId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public List<Guid>? RoleIds { get; set; }
        }
    }
}
