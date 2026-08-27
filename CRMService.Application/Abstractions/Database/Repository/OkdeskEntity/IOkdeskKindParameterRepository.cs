using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskKindParameterRepository
    {
        Task<List<KindsParameter>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
