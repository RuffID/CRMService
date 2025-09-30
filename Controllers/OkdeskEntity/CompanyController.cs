using CRMService.Interfaces.Repository;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(IUnitOfWork unitOfWork, EntitySyncService sync, CompanyService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] int id, CancellationToken ct)
        {
            Company? company = await unitOfWork.Company.GetItemById(id, false, ct);

            if (company == null)
                return NotFound();

            return Ok(company.ToDto());
        }

        [HttpGet("by_category")]
        public async Task<IActionResult> GetCompaniesByCategory([FromQuery] string categoryCode, [FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Company> companies = await unitOfWork.Company.GetCompaniesByCategoryCode(categoryCode: categoryCode, startIndexCompany: startIndex, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct: ct);

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
        public async Task<IActionResult> UpdateCompaniesFromCloudApi([FromQuery] int startIndexCategory, int startIndexCompany, CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudApi(startIndexCategory, startIndexCompany, ct);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCompaniesFromCloudDb([FromQuery] int startIndexCategory, int startIndexCompany, CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudDb(startIndexCategory, startIndexCompany, ct);
            });

            return NoContent();
        }
    }
}
