using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IOkdeskRoleRepository : IGetItemByIdRepository<OkdeskRole, int>, IUpsertItemByIdRepository<OkdeskRole, int>, ICreateItemRepository<OkdeskRole>
    {
        Task<OkdeskRole?> GetRoleByName(string name, bool asNoTracking = false);
    }
}