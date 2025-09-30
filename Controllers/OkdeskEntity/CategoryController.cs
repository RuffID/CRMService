using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Dto.Mappers.OkdeskEntity;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IUnitOfWork unitOfWork, CompanyCategoryService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetCategories([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAndSortById(predicate: c => c.Id >= startIndex, asNoTracking: true, ct: ct);

            return Ok(categories.ToDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery] int id, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemById(id, asNoTracking: true, ct: ct);

            if (category == null)
                return NotFound();
                        
            return Ok(category.ToDto());
        }        

        [HttpPut, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCategory([FromBody] CompanyCategoryDto updatedCategory, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemById(updatedCategory.Id, asNoTracking: true, ct: ct);

            if (category == null)
                return NotFound();

            await unitOfWork.CompanyCategory.Upsert(updatedCategory.ToEntity(), ct);

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPost, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> CreateCategory([FromBody] CompanyCategoryDto categoryCreate, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemById(categoryCreate.Id, asNoTracking: true, ct: ct);

            if (category != null)
                return Conflict("Id: already exist");

            unitOfWork.CompanyCategory.Create(categoryCreate.ToEntity());

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCategoriesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateCategoriesFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("check_anonymous_category"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> CheckAnonymousCategory(CancellationToken ct)
        {
            await service.CheckAnonymousCategory(ct);

            return NoContent();
        }
    }
}
