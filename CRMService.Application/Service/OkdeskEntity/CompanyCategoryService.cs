using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class CompanyCategoryService(IOkdeskCompanyDirectorySource okdeskUnitOfWork, ICompanyDirectoryUnitOfWork unitOfWork, EntitySyncService sync, ILogger<CompanyCategoryService> logger)
    {
        public Task<List<CompanyCategory>> GetCategoriesAsync(CancellationToken ct = default) =>
            unitOfWork.CompanyCategory.GetItemsReadOnlyAsync(ct);

        public Task<CompanyCategory?> GetCategoryAsync(int id, CancellationToken ct = default) =>
            unitOfWork.CompanyCategory.GetItemByIdReadOnlyAsync(id, ct);

        public async Task<bool> UpdateCategoryAsync(CompanyCategory updatedCategory, CancellationToken ct)
        {
            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByIdAsync(updatedCategory.Id, ct);
            if (category == null)
                return false;

            category.CopyData(updatedCategory);
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CreateCategoryAsync(CompanyCategory category, CancellationToken ct)
        {
            CompanyCategory? existing = await unitOfWork.CompanyCategory.GetItemByIdReadOnlyAsync(category.Id, ct);
            if (existing != null)
                return false;

            unitOfWork.CompanyCategory.Create(category);
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        private async Task<List<CompanyCategory>> GetCategoriesFromCloudDb(CancellationToken ct)
        {
            List<CompanyCategory> categories = await okdeskUnitOfWork.CompanyCategory.GetAllReadOnlyAsync(ct);

            return categories.OrderBy(x => x.Id).ToList();
        }

        public async Task CheckAnonymousCategory(CancellationToken ct)
        {
            // Создание категории с нулевым id которой нет в базе окдеска, но по которой ищутся клиенты без категории
            // Это нужно для первого запуска сервера
            CompanyCategory no_category = new() { Name = "Без категории", Code = "no_category", Color = "#FFFFFF" };
            CompanyCategory? noCategoryFromDb = await unitOfWork.CompanyCategory.GetByCodeReadOnlyAsync(no_category.Code, ct);
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
                    CompanyCategory? existingCategory = await unitOfWork.CompanyCategory.GetByCodeAsync(category.Code, ct);

                    if (existingCategory == null)
                    {
                        category.Id = 0;
                        unitOfWork.CompanyCategory.Create(category);
                    }
                    else
                        existingCategory.CopyData(category);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update company categories complete.", nameof(UpdateCategoriesFromCloudDb));
        }
    }
}
