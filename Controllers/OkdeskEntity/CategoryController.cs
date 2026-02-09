using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Abstractions.Database.Repository;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IUnitOfWork unitOfWork, CompanyCategoryService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetCategories(CancellationToken ct = default)
        {
            List<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(categories.ToDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery] int id, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByIdAsync(id, asNoTracking: true, ct: ct);

            if (category == null)
                return NotFound();

            return Ok(category.ToDto());
        }

        [HttpPut, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCategory([FromBody] CompanyCategoryDto updatedCategory, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByIdAsync(updatedCategory.Id, ct: ct);

            if (category == null)
                return NotFound();

            category.CopyData(updatedCategory.ToEntity());

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPost, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> CreateCategory([FromBody] CompanyCategoryDto categoryCreate, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByIdAsync(categoryCreate.Id, asNoTracking: true, ct: ct);

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
