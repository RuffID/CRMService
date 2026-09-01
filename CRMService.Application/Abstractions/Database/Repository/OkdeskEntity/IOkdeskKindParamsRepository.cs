using CRMService.Application.Models.OkdeskSource;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskKindParamsRepository
    {
        Task<List<OkdeskKindParameterConnectionRecord>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
