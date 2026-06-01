using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(IUnitOfWork unitOfWork, CompanyService service, BackgroundUpdateService backgroundUpdateService) : Controller
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
            await service.UpdateCompanyFromCloudApi(companyId, ct);

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateCompaniesFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateCompaniesFromCloudApi),
                (provider, token) => provider.GetRequiredService<CompanyService>().UpdateCompaniesFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_companies_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateCompaniesFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateCompaniesFromCloudDb),
                (provider, token) => provider.GetRequiredService<CompanyService>().UpdateCompaniesFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
