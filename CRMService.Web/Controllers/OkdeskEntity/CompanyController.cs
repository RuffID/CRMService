using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(IUnitOfWork unitOfWork, EntitySyncService sync, CompanyService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] int id, CancellationToken ct)
        {
            Company? company = await unitOfWork.Company.GetItemByIdAsync(id, asNoTracking: true, ct: ct);

            if (company == null)
                return NotFound();

            return Ok(company.ToDto());
        }

        [HttpGet("by_category")]
        public async Task<IActionResult> GetCompaniesByCategory([FromQuery] string categoryCode, CancellationToken ct = default)
        {
            List<Company> companies = await unitOfWork.Company.GetItemsByPredicateAsync(predicate: c => c.Category!.Code == categoryCode, ct: ct);

            return Ok(companies.ToDto());
        }

        [HttpPut("update_from_api")]
        public async Task<IActionResult> UpdateCompanyFromCloudApi([FromQuery] int companyId, CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompanyFromCloudApi(companyId, ct);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCompaniesFromCloudApi(CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudApi(ct);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCompaniesFromCloudDb(CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudDb(ct);
            });

            return NoContent();
        }
    }
}