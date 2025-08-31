using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Service.Sync;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(IMapper mapper, IUnitOfWork unitOfWork, EntitySyncService sync, CompanyService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] int id, CancellationToken ct)
        {
            Company? companyFromDB = await unitOfWork.Company.GetItemById(id, false, ct);

            if (companyFromDB == null)
                return NotFound();

            return Ok(mapper.Map<CompanyDto>(companyFromDB));
        }

        [HttpGet("by_category")]
        public async Task<IActionResult> GetCompaniesByCategory([FromQuery] string categoryCode, [FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Company> companies = await unitOfWork.Company.GetCompaniesByCategoryCode(categoryCode: categoryCode, startIndexCompany: startIndex, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct: ct);

            return Ok(mapper.Map<List<CompanyDto>>(companies));
        }        

        [HttpPut("update_company_from_cloud_api")]
        public async Task<IActionResult> UpdateCompanyFromCloudApi([FromQuery] int companyId, CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompanyFromCloudApi(companyId, ct);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateCompaniesFromCloudApi([FromQuery] int startIndexCategory, int startIndexCompany, CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudApi(startIndexCategory, startIndexCompany, ct);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
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
