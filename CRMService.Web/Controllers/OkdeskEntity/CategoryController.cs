using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(CompanyCategoryService service, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetCategories(CancellationToken ct = default)
        {
            List<CompanyCategory> categories = await service.GetCategoriesAsync(ct);

            return Ok(categories.ToDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery] int id, CancellationToken ct)
        {
            CompanyCategory? category = await service.GetCategoryAsync(id, ct);

            if (category == null)
                return NotFound();

            return Ok(category.ToDto());
        }

        [HttpPut, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateCategory([FromBody] CompanyCategoryDto updatedCategory, CancellationToken ct)
        {
            if (!await service.UpdateCategoryAsync(updatedCategory.ToEntity(), ct))
                return NotFound();

            return NoContent();
        }

        [HttpPost, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> CreateCategory([FromBody] CompanyCategoryDto categoryCreate, CancellationToken ct)
        {
            if (!await service.CreateCategoryAsync(categoryCreate.ToEntity(), ct))
                return Conflict("Id: already exist");

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateCategoriesFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateCategoriesFromCloudDb),
                (provider, token) => provider.GetRequiredService<CompanyCategoryService>().UpdateCategoriesFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("check_anonymous_category"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> CheckAnonymousCategory(CancellationToken ct)
        {
            await service.CheckAnonymousCategory(ct);

            return NoContent();
        }
    }
}
