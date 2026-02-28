using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Dto.Mappers.Authorize;
using CRMService.Models.Request;
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
            if (roleIds.Count == 0)
                return ServiceResult.Fail(400, "Выберите хотя бы одну роль.");

            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByPredicateAsync(r => roleIds.Contains(r.Id), asNoTracking: false, ct: ct);
            if (roles.Count != roleIds.Count)
                return ServiceResult.Fail(400, "Одна или несколько ролей не найдены.");

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

            List<Guid> roleIds = (request.RoleIds ?? []).Distinct().ToList();
            if (roleIds.Count == 0)
                return ServiceResult.Fail(400, "Выберите хотя бы одну роль.");

            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByPredicateAsync(r => roleIds.Contains(r.Id), asNoTracking: false, ct: ct);
            if (roles.Count != roleIds.Count)
                return ServiceResult.Fail(400, "Одна или несколько ролей не найдены.");

            user.Name = name;
            user.Login = login;

            if (!string.IsNullOrWhiteSpace(password))
                user.Password = hasher.Hash(password);

            user.Roles.Clear();
            foreach (CrmRole role in roles)
                user.Roles.Add(role);

            await unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetUserActiveAsync(Guid currentUserId, Guid userId, bool isActive, CancellationToken ct)
        {
            if (userId == Guid.Empty)
                return ServiceResult.Fail(400, "Не указан пользователь.");

            if (!isActive && currentUserId == userId)
                return ServiceResult.Fail(400, "Нельзя деактивировать текущего пользователя.");

            User? user = await unitOfWork.User.GetItemByIdAsync(userId, asNoTracking: false, ct: ct);
            if (user == null)
                return ServiceResult.Fail(404, "Пользователь не найден.");

            if (user.Active == isActive)
                return ServiceResult.Ok();

            user.Active = isActive;
            await unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok();
        }
    }
}
