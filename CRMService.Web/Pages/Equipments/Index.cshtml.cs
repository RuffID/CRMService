using CRMService.Application.Service.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.Constants;
using CRMService.Web.Core.Mappers;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages.Equipments
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class IndexModel(
        EquipmentService equipmentService,
        MaintenanceEntityService maintenanceEntityService,
        CompanyService companyService,
        KindService kindService,
        ManufacturerService manufacturerService,
        ModelService modelService) : PageModel
    {
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
    }
}
