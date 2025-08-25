using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.Entity;
using System.Data;

namespace CRMService.Service.Entity
{
    public class CompanyCategoryService(PGSelect pGSelect, IUnitOfWork unitOfWork, ILoggerFactory logger)
    {
        private readonly PGSelect _pGSelect = pGSelect;
        private readonly ILogger<CompanyCategoryService> _logger = logger.CreateLogger<CompanyCategoryService>();
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private async Task<List<CompanyCategory>?> GetCategoriesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM company_categories;";
            DataSet ds = await _pGSelect.Select(sqlCommand);
            DataTable? categoryTable = ds.Tables["Table"];
            if (categoryTable == null)
                return null;

            return categoryTable.AsEnumerable().
                    Select(category => new CompanyCategory
                    {
                        Id = categoryTable.Rows.IndexOf(category) + 1,
                        Name = category.Field<string>("name"),
                        Code = category.Field<string>("code"),
                        Color = category.Field<string>("color")
                    }).ToList();
        }

        public async Task CheckAnonymousCategory()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting check anonymous company category.", nameof(CheckAnonymousCategory));

            // Создание категории с нулевым id которой нет в базе окдеска, но по которой ищутся клиенты без категории
            // Это нужно для первого запуска сервера
            CompanyCategory no_category = new() { Id = 0, Name = "Без категории", Code = "no_category", Color = "#FFFFFF" };
            if (await _unitOfWork.CompanyCategory.GetItem(no_category, false) == null)
            {
                _unitOfWork.CompanyCategory.Create(no_category);
                await _unitOfWork.SaveAsync();
            }

            _logger.LogInformation("[Method:{MethodName}] Check anonymous company category completed.", nameof(CheckAnonymousCategory));
        }

        public async Task UpdateCategoriesFromCloudDb()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating company categories.", nameof(UpdateCategoriesFromCloudDb));

            List<CompanyCategory>? categories = await GetCategoriesFromCloudDb();

            if (categories == null || categories.Count == 0)
                return;

            await _unitOfWork.CompanyCategory.CreateOrUpdate(categories);

            await _unitOfWork.SaveAsync();

            _logger.LogInformation("[Method:{MethodName}] Company categories update completed.", nameof(UpdateCategoriesFromCloudDb));
        }
    }
}
