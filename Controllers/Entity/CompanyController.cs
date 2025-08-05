using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CRMService.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using CRMService.Core;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Service.Sync;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class CompanyController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IUnitOfWorkEntities unitOfWork, EntitySyncService sync, CompanyService service) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] int id)
        {
            var companyFromDB = mapper.Map<CompanyDto>(await unitOfWork.Company.GetCompanyById(id));

            if (companyFromDB == null)
                return NotFound("Company not found.");

            return Ok(companyFromDB);
        }

        [HttpGet("by_category")]
        public async Task<IActionResult> GetCompaniesByCategory([FromQuery] string categoryCode, [FromQuery] int startIndex = 0)
        {
            IEnumerable<CompanyDto>? companies = mapper.Map<IEnumerable<CompanyDto>>(await unitOfWork.Company.GetCompaniesByCategoryCode(categoryCode, startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (companies == null || !companies.Any())
                return NotFound();

            return Ok(companies);
        }        

        [HttpPut("update_company_from_cloud_api")]
        public async Task<IActionResult> UpdateCompanyFromCloudApi([FromQuery] int companyId)
        {
            if (companyId == 0)
                BadRequest("Company id not set.");

            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompanyFromCloudApi(companyId);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateCompaniesFromCloudApi([FromQuery] int startIndexCategory = 0, int startIndexCompany = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudApi(startIndexCategory, startIndexCompany);
            });

            return NoContent();
        }

        [HttpPut("update_companies_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateCompaniesFromCloudDb([FromQuery] int startIndexCategory = 0, int startIndexCompany = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateCompaniesFromCloudDb(startIndexCategory, startIndexCompany);
            });

            return NoContent();
        }
    }
}
