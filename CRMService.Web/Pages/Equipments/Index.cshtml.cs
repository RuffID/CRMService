using CRMService.Application.Abstractions.Entity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.Authorization;
using CRMService.Web.Core.Mappers;
using CRMService.Web.Service.Attributes;
using CRMService.Web.Service.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages.Equipments
{
    [CookieAuthorize]
    [LoadUser]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class IndexModel(
        EquipmentService equipmentService,
        EquipmentCloudDbUpdateService equipmentCloudDbUpdateService,
        MaintenanceEntityService maintenanceEntityService,
        CompanyService companyService,
        KindService kindService,
        ManufacturerService manufacturerService,
        ModelService modelService) : PageModel, IHasCurrentUser
    {
        public User CurrentUser { get; set; } = null!;
        public bool CanStartCloudDbUpdate => User.IsInRole(RolesConstants.ADMIN);

        public async Task<IActionResult> OnGetListAsync([FromQuery] EquipmentListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await equipmentService.GetEquipmentListPageAsync(request, ct));
        }

        public async Task<IActionResult> OnGetExactCountAsync([FromQuery] EquipmentListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await equipmentService.GetEquipmentExactCountAsync(request, ct));
        }

        public async Task<IActionResult> OnGetCompanyLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await companyService.GetCompanyLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetTypeLookupAsync([FromQuery] EquipmentLookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await kindService.GetKindLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetManufacturerLookupAsync([FromQuery] EquipmentLookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await manufacturerService.GetManufacturerLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetModelLookupAsync([FromQuery] EquipmentLookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await modelService.GetModelLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetMaintenanceEntityLookupAsync([FromQuery] EquipmentLookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await maintenanceEntityService.GetMaintenanceEntityLookupAsync(request, ct));
        }

        public IActionResult OnGetCloudDbUpdateState()
        {
            if (!CanStartCloudDbUpdate)
                return Forbid();

            return JsonResultMapper.ToJsonResult(ServiceResult<EquipmentCloudDbUpdateStateDto>.Ok(equipmentCloudDbUpdateService.GetState()));
        }

        public IActionResult OnPostStartCloudDbUpdate()
        {
            if (!CanStartCloudDbUpdate)
                return Forbid();

            return JsonResultMapper.ToJsonResult(equipmentCloudDbUpdateService.TryStart(CurrentUser.Name));
        }
    }
}
