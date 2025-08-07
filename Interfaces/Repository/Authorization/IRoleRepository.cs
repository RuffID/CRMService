using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IRoleRepository : IGetRepository<Role>, ICreateRepository<Role>
    {
    }
}
