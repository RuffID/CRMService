using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Dto.Entity;
using CRMService.Models.ConfigClass;

namespace CRMService.Controllers.Entity
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

            List<CompanyCategoryDto> dtos = categories.Select(c => new CompanyCategoryDto()
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Color = c.Color,
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery] int id, CancellationToken ct)
        {
            CompanyCategory? categoryFromDb = await unitOfWork.CompanyCategory.GetItemById(id, asNoTracking: true, ct: ct);

            if (categoryFromDb == null)
                return NotFound();

            CompanyCategoryDto dto = new()
            {
                Id = categoryFromDb.Id,
                Name = categoryFromDb.Name,
                Code = categoryFromDb.Code,
                Color = categoryFromDb.Color
            };

            return Ok(dto);
        }        

        [HttpPut, Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateCategory([FromBody] CompanyCategoryDto updatedCategory, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByPredicate(predicate: cc => cc.Code == updatedCategory.Code, asNoTracking: true, ct: ct);

            if (category == null)
                return NotFound();

            category = new()
            {
                Id = updatedCategory.Id,
                Name = updatedCategory.Name ?? "",
                Code = updatedCategory.Code ?? "",
                Color = updatedCategory.Color ?? ""
            };

            await unitOfWork.CompanyCategory.Upsert(category, ct);

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPost, Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> CreateCategory([FromBody] CompanyCategoryDto categoryCreate, CancellationToken ct)
        {
            if (await unitOfWork.CompanyCategory.GetItemById(categoryCreate.Id, true, ct) != null)
                return Conflict("Id: already exist");

            CompanyCategory? category = new()
            {
                Id = categoryCreate.Id,
                Name = categoryCreate.Name,
                Code = categoryCreate.Code,
                Color = categoryCreate.Color
            };

            unitOfWork.CompanyCategory.Create(category);

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateCategoriesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateCategoriesFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("check_anonymous_category"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> CheckAnonymousCategory(CancellationToken ct)
        {
            await service.CheckAnonymousCategory(ct);

            return NoContent();
        }
    }
}
