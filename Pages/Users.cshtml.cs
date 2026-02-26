using CRMService.Abstractions.Entity;
using CRMService.Models.Authorization;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Responses.Results;
using CRMService.Service.Attributes;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Pages
{
    [CookieAuthorize]
    [LoadUser]
    public class UsersModel(UserService userService, RoleService roleService) : PageModel, IHasCurrentUser
    {
        public User CurrentUser { get; set; } = null!;

        public IActionResult OnGet()
        {
            if (!userService.IsAdmin(CurrentUser))
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnGetListAsync(bool includeInactive = true, CancellationToken ct = default)
        {
            if (!userService.IsAdmin(CurrentUser))
                return JsonResultMapper.ToJsonResult(ServiceResult<List<object>>.Fail(403, "Недостаточно прав."));

            ServiceResult<List<Models.Dto.Authorization.UserDto>> result = await userService.GetUsersAsync(ct);
            if (!result.Success)
                return JsonResultMapper.ToJsonResult(ServiceResult<List<object>>.Fail(result.Error!.StatusCode, result.Error!.Message));

            List<Models.Dto.Authorization.UserDto> users = result.Data ?? [];
            if (!includeInactive)
                users = users.Where(x => x.Active).ToList();

            var data = users
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Login,
                    u.Active,
                    Roles = u.Roles.Select(r => new { r.Id, r.Name }).OrderBy(r => r.Name).ToList()
                })
                .ToList();

            return JsonResultMapper.ToJsonResult(ServiceResult<List<object>>.Ok(data.Cast<object>().ToList()));
        }

        public async Task<IActionResult> OnGetRolesAsync(CancellationToken ct)
        {
            if (!userService.IsAdmin(CurrentUser))
                return JsonResultMapper.ToJsonResult(ServiceResult<List<object>>.Fail(403, "Недостаточно прав."));

            ServiceResult<List<Models.Dto.Authorization.CrmRoleDto>> result = await roleService.GetRolesAsync(ct);
            if (!result.Success)
                return JsonResultMapper.ToJsonResult(ServiceResult<List<object>>.Fail(result.Error!.StatusCode, result.Error!.Message));

            var data = (result.Data ?? [])
                .OrderBy(r => r.Name)
                .Select(r => new { r.Id, r.Name })
                .ToList();

            return JsonResultMapper.ToJsonResult(ServiceResult<List<object>>.Ok(data.Cast<object>().ToList()));
        }

        public async Task<IActionResult> OnPostCreateAsync([FromBody] UserService.CreateUserRequest request, CancellationToken ct)
        {
            if (!userService.IsAdmin(CurrentUser))
                return JsonResultMapper.ToJsonResult(ServiceResult.Fail(403, "Недостаточно прав."));

            ServiceResult result = await userService.CreateUserAsync(request, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostUpdateAsync([FromBody] UserService.UpdateUserRequest request, CancellationToken ct)
        {
            if (!userService.IsAdmin(CurrentUser))
                return JsonResultMapper.ToJsonResult(ServiceResult.Fail(403, "Недостаточно прав."));

            ServiceResult result = await userService.UpdateUserAsync(request, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostDeactivateAsync([FromBody] DeactivateUserRequest request, CancellationToken ct)
        {
            if (!userService.IsAdmin(CurrentUser))
                return JsonResultMapper.ToJsonResult(ServiceResult.Fail(403, "Недостаточно прав."));

            ServiceResult result = await userService.DeactivateUserAsync(CurrentUser.Id, request.UserId, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public sealed class DeactivateUserRequest
        {
            public Guid UserId { get; set; }
        }
    }
}
