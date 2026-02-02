using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class CompanyCategoryService(PGSelect pGSelect, IUnitOfWork unitOfWork, ILoggerFactory logger)
    {
        private readonly PGSelect _pGSelect = pGSelect;
        private readonly ILogger<CompanyCategoryService> _logger = logger.CreateLogger<CompanyCategoryService>();
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private async Task<List<CompanyCategory>> GetCategoriesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM company_categories;";
            DataSet ds = await _pGSelect.Select(sqlCommand);
            DataTable? categoryTable = ds.Tables["Table"];
            if (categoryTable == null)
                return new();

            return categoryTable.AsEnumerable().
                    Select(category => new CompanyCategory
                    {
                        Id = categoryTable.Rows.IndexOf(category) + 1,
                        Name = category.Field<string>("name") ?? string.Empty,
                        Code = category.Field<string>("code") ?? string.Empty,
                        Color = category.Field<string>("color") ?? string.Empty
                    }).ToList();
        }

        public async Task CheckAnonymousCategory(CancellationToken ct)
        {
            // Создание категории с нулевым id которой нет в базе окдеска, но по которой ищутся клиенты без категории
            // Это нужно для первого запуска сервера
            CompanyCategory no_category = new() { Id = 0, Name = "Без категории", Code = "no_category", Color = "#FFFFFF" };
            CompanyCategory? noCategoryFromDb = await _unitOfWork.CompanyCategory.GetItemByIdAsync(no_category.Id, true, ct: ct);
            if (noCategoryFromDb == null)
            {
                _unitOfWork.CompanyCategory.Create(no_category);
                await _unitOfWork.SaveAsync(ct);
            }
        }

        public async Task UpdateCategoriesFromCloudDb(CancellationToken ct)
        {
            List<CompanyCategory> categories = await GetCategoriesFromCloudDb();

            if (categories.Count == 0)
                return;

            foreach (CompanyCategory category in categories)
            {
                CompanyCategory? existingCategory = await _unitOfWork.CompanyCategory.GetItemByIdAsync(category.Id, ct: ct);

                if (existingCategory == null)
                    _unitOfWork.CompanyCategory.Create(category);
                else
                    existingCategory.CopyData(category);
            }

            await _unitOfWork.SaveAsync(ct);
        }
    }
}
