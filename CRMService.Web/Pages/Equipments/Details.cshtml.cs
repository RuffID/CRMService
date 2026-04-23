using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Web.Core.Mappers;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace CRMService.Web.Pages.Equipments
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class DetailsModel(EquipmentService equipmentService, IOptions<ApiEndpointOptions> endpoint) : PageModel
    {
        public string BackUrl { get; private set; } = "/equipments?page=1";
        public int EquipmentId { get; private set; }
        public string OkdeskEquipmentUrl { get; private set; } = string.Empty;

        public void OnGet(int id, [FromQuery(Name = "page")] int? page)
        {
            EquipmentId = id;
            int listPage = page.HasValue && page.Value > 0 ? page.Value : 1;
            BackUrl = $"/equipments?page={listPage}";
            OkdeskEquipmentUrl = $"{endpoint.Value.OkdeskDomainUrl.TrimEnd('/')}/equipments/{id}";
        }

        public async Task<IActionResult> OnGetDetailsAsync(int id, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await equipmentService.GetEquipmentDetailsAsync(id, ct));
        }

        public async Task<IActionResult> OnPostUpdateFromCloudApiAsync(int id, CancellationToken ct)
        {
            if (id <= 0)
                return JsonResultMapper.ToJsonResult(ServiceResult.Fail(400, "Идентификатор оборудования должен быть больше нуля."));

            await equipmentService.UpdateEquipmentFromCloudApiAsync(id, ct);

            return JsonResultMapper.ToJsonResult(ServiceResult.Ok());
        }
    }
}
