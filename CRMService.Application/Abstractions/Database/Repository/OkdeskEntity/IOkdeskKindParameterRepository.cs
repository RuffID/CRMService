using CRMService.Application.Models.OkdeskSource;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskKindParameterRepository
    {
        Task<List<OkdeskKindParameterRecord>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
