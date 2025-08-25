using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
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
    public class CategoryController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IUnitOfWork unitOfWork, CompanyCategoryService service) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetCategories([FromQuery] int startIndex = 0)
        {
            var categories = mapper.Map<IEnumerable<CategoryDto>>(await unitOfWork.CompanyCategory.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (categories == null || !categories.Any())
                return NotFound();

            return Ok(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery] string code)
        {
            var category = mapper.Map<CategoryDto>(await unitOfWork.CompanyCategory.GetItem(new CompanyCategory() { Code = code }));

            if (category == null)
                return NotFound();

            return Ok(category);
        }        

        [HttpPut, Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryDto updatedCategory)
        {
            if (string.IsNullOrEmpty(updatedCategory.Code) || string.IsNullOrEmpty(updatedCategory.Color) || string.IsNullOrEmpty(updatedCategory.Name))
                return BadRequest("The fields for creating an object are not filled in.");

            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItem(mapper.Map<CompanyCategory>(updatedCategory));
            if (category == null)
                return BadRequest("Category not found.");

            CompanyCategory categoryMap = mapper.Map<CompanyCategory>(updatedCategory);

            unitOfWork.CompanyCategory.Update(category, categoryMap);

            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPost, Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryCreate)
        {
            if (string.IsNullOrEmpty(categoryCreate.Code) || string.IsNullOrEmpty(categoryCreate.Color) || string.IsNullOrEmpty(categoryCreate.Name))
                return BadRequest("The fields for creating an object are not filled in.");

            if (await unitOfWork.CompanyCategory.GetItem(mapper.Map<CompanyCategory>(categoryCreate)) != null)
                return BadRequest("Category already exists.");

            var categoryMap = mapper.Map<CompanyCategory>(categoryCreate);

            unitOfWork.CompanyCategory.Create(categoryMap);

            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateCategoriesFromCloudDb()
        {
            await service.UpdateCategoriesFromCloudDb();

            return NoContent();
        }

        [HttpPut("check_anonymous_category"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> CheckAnonymousCategory()
        {
            await service.CheckAnonymousCategory();

            return NoContent();
        }
    }
}
