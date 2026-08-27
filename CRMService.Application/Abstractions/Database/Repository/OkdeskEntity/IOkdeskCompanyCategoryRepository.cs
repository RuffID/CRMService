using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskCompanyCategoryRepository
    {
        Task<List<CompanyCategory>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
