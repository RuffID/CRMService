using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IMapper mapper, IUnitOfWork unitOfWork, CompanyCategoryService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetCategories([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAndSortById(predicate: c => c.Id >= startIndex, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<CategoryDto>>(categories));
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery] string code, CancellationToken ct)
        {
            CompanyCategory? categoryFromDb = await unitOfWork.CompanyCategory.GetItemByCode(code, true, ct);

            if (categoryFromDb == null)
                return NotFound();

            return Ok(mapper.Map<CategoryDto>(categoryFromDb));
        }        

        [HttpPut, Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryDto updatedCategory, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByCode(updatedCategory.Code, true, ct);

            if (category == null)
                return NotFound();

            await unitOfWork.CompanyCategory.UpsertByCode(mapper.Map<CompanyCategory>(updatedCategory), ct);

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPost, Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryCreate, CancellationToken ct)
        {
            if (await unitOfWork.CompanyCategory.GetItemById(categoryCreate.Id, true, ct) != null)
                return Conflict("Id: already exist");

            CompanyCategory? categoryMap = mapper.Map<CompanyCategory>(categoryCreate);

            unitOfWork.CompanyCategory.Create(categoryMap);

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateCategoriesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateCategoriesFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("check_anonymous_category"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> CheckAnonymousCategory(CancellationToken ct)
        {
            await service.CheckAnonymousCategory(ct);

            return NoContent();
        }
    }
}
