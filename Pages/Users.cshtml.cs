using CRMService.Abstractions.Entity;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Request;
using CRMService.Models.Responses.Results;
using CRMService.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Pages
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    [LoadUser]
    public class UsersModel(UserService userService, RoleService roleService) : PageModel, IHasCurrentUser
    {
        public User CurrentUser { get; set; } = null!;

        public async Task<IActionResult> OnGetListAsync(bool includeInactive = true, CancellationToken ct = default)
        {
            ServiceResult<List<UserDto>> result = await userService.GetUsersAsync(ct);
            if (!result.Success)
                return JsonResultMapper.ToJsonResult(ServiceResult<List<UserListItemDto>>.Fail(result.Error!.StatusCode, result.Error!.Message));

            List<UserDto> users = result.Data ?? [];
            if (!includeInactive)
                users = users.Where(x => x.Active).ToList();

            List<UserListItemDto> data = users
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Login = u.Login,
                    Active = u.Active,
                    Roles = u.Roles
                        .OrderBy(r => r.Name)
                        .Select(r => new RoleListItemDto
                        {
                            Id = r.Id,
                            Name = r.Name
                        })
                        .ToList()
                })
                .ToList();

            return JsonResultMapper.ToJsonResult(ServiceResult<List<UserListItemDto>>.Ok(data));
        }

        public async Task<IActionResult> OnGetRolesAsync(CancellationToken ct)
        {
            ServiceResult<List<CrmRoleDto>> result = await roleService.GetRolesAsync(ct);
            if (!result.Success)
                return JsonResultMapper.ToJsonResult(ServiceResult<List<RoleListItemDto>>.Fail(result.Error!.StatusCode, result.Error!.Message));

            List<RoleListItemDto> data = (result.Data ?? [])
                .OrderBy(r => r.Name)
                .Select(r => new RoleListItemDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToList();

            return JsonResultMapper.ToJsonResult(ServiceResult<List<RoleListItemDto>>.Ok(data));
        }

        public async Task<IActionResult> OnPostCreateAsync([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            ServiceResult result = await userService.CreateUserAsync(request, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostUpdateAsync([FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            ServiceResult result = await userService.UpdateUserAsync(request, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSetActiveAsync([FromBody] SetUserActiveRequest request, CancellationToken ct)
        {
            ServiceResult result = await userService.SetUserActiveAsync(CurrentUser.Id, request.UserId, request.IsActive, ct);
            return JsonResultMapper.ToJsonResult(result);
        }
    }
}
