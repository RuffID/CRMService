using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class CompanyCategoryService(IOkdeskUnitOfWork okdeskUnitOfWork, IUnitOfWork unitOfWork, EntitySyncService sync, ILogger<CompanyCategoryService> logger)
    {
        private async Task<List<CompanyCategory>> GetCategoriesFromCloudDb(CancellationToken ct)
        {
            List<CompanyCategory> categories = await okdeskUnitOfWork.CompanyCategory.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return categories.OrderBy(x => x.Id).ToList();
        }

        public async Task CheckAnonymousCategory(CancellationToken ct)
        {
            // Создание категории с нулевым id которой нет в базе окдеска, но по которой ищутся клиенты без категории
            // Это нужно для первого запуска сервера
            CompanyCategory no_category = new() { Name = "Без категории", Code = "no_category", Color = "#FFFFFF" };
            CompanyCategory? noCategoryFromDb = await unitOfWork.CompanyCategory.GetItemByPredicateAsync(c => c.Code == no_category.Code, true, ct: ct);
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
                    CompanyCategory? existingCategory = await unitOfWork.CompanyCategory.GetItemByPredicateAsync(c => c.Code == category.Code, ct: ct);

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