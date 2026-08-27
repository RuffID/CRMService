using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskCompanyRepository
    {
        Task<List<Company>> GetAllWithCategoryReadOnlyAsync(CancellationToken ct = default);
    }
}
