using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskManufacturerRepository
    {
        Task<List<Manufacturer>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
