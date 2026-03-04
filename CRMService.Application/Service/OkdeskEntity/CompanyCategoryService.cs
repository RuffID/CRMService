using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using System.Data;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class CompanyCategoryService(IPostgresSelect pGSelect, IUnitOfWork unitOfWork, EntitySyncService sync, ILogger<CompanyCategoryService> logger)
    {
        private async Task<List<CompanyCategory>> GetCategoriesFromCloudDb(CancellationToken ct)
        {
            string sqlCommand = "SELECT * FROM company_categories;";
            DataSet ds = await pGSelect.Select(sqlCommand, ct);
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
            CompanyCategory? noCategoryFromDb = await unitOfWork.CompanyCategory.GetItemByIdAsync(no_category.Id, true, ct: ct);
            if (noCategoryFromDb == null)
            {
                unitOfWork.CompanyCategory.Create(no_category);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }

        public async Task UpdateCategoriesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update company categories from DB.", nameof(UpdateCategoriesFromCloudDb));

            List<CompanyCategory> categories = await GetCategoriesFromCloudDb(ct);

            if (categories.Count == 0)
                return;

            foreach (CompanyCategory category in categories)
            {
                await sync.RunExclusive(category, async () =>
                {
                    CompanyCategory? existingCategory = await unitOfWork.CompanyCategory.GetItemByIdAsync(category.Id, ct: ct);

                    if (existingCategory == null)
                        unitOfWork.CompanyCategory.Create(category);
                    else
                        existingCategory.CopyData(category);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update company categories complete.", nameof(UpdateCategoriesFromCloudDb));
        }
    }
}