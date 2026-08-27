using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskKindParamsRepository
    {
        Task<List<KindParam>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
