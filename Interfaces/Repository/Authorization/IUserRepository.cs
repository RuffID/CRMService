using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Auth
{
    public interface IUserRepository : IGetRepository<User>, ICreateRepository<User>, IUpdateRepository<User>
    {

    }
}
