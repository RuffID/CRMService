using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using Microsoft.EntityFrameworkCore;
using CRMService.Application.Common.Mapping.Authorize;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.Authorization
{
    public class UserService(IUnitOfWork unitOfWork, Hasher hasher)
    {
        public async Task<ServiceResult<List<UserDto>>> GetUsersAsync(CancellationToken ct)
        {
            List<User> users = await unitOfWork.User.GetItemsByPredicateAsync(
                asNoTracking: true,
                include: q => q.Include(u => u.Roles).Include(u => u.Employee),
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

            ServiceResult<Employee?> employeeResult = await ResolveEmployeeAsync(request.EmployeeId, userId: null, ct);
            if (!employeeResult.Success)
                return ServiceResult.Fail(employeeResult.Error!.StatusCode, employeeResult.Error.Message);

            User user = new()
            {
                Name = name,
                Login = login,
                Password = hasher.Hash(password),
                Active = true,
                EmployeeId = employeeResult.Data?.Id,
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

            ServiceResult<Employee?> employeeResult = await ResolveEmployeeAsync(request.EmployeeId, request.UserId, ct);
            if (!employeeResult.Success)
                return ServiceResult.Fail(employeeResult.Error!.StatusCode, employeeResult.Error.Message);

            user.Name = name;
            user.Login = login;
            user.EmployeeId = employeeResult.Data?.Id;

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

        private async Task<ServiceResult<Employee?>> ResolveEmployeeAsync(int? employeeId, Guid? userId, CancellationToken ct)
        {
            if (employeeId == null)
                return ServiceResult<Employee?>.Ok(null);

            if (employeeId <= 0)
                return ServiceResult<Employee?>.Fail(400, "Некорректный сотрудник Okdesk.");

            Employee? employee = await unitOfWork.Employee.GetItemByIdAsync(employeeId.Value, asNoTracking: true, ct: ct);
            if (employee == null)
                return ServiceResult<Employee?>.Fail(404, "Сотрудник Okdesk не найден.");

            User? existingUser = userId == null
                ? await unitOfWork.User.GetItemByPredicateAsync(
                    user => user.EmployeeId == employeeId,
                    asNoTracking: true,
                    ct: ct)
                : await unitOfWork.User.GetItemByPredicateAsync(
                    user => user.EmployeeId == employeeId && user.Id != userId.Value,
                    asNoTracking: true,
                    ct: ct);

            if (existingUser != null)
                return ServiceResult<Employee?>.Fail(409, "Этот сотрудник Okdesk уже привязан к другому пользователю.");

            return ServiceResult<Employee?>.Ok(employee);
        }
    }
}