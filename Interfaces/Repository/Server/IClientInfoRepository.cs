using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Server;

namespace CRMService.Interfaces.Repository.Server
{
    public interface IClientInfoRepository : IGetRepository<ClientAppInfo>, ICreateRepository<ClientAppInfo>, IDeleteRepository<ClientAppInfo>
    {
        Task<ClientAppInfo?> GetLatestReleaseInfo();
    }
}
