using CRMService.Interfaces.Repository.Server;

namespace CRMService.Interfaces.Repository
{
    public interface IUnitOfWorkServerInfo : IDisposable
    {
        IClientInfoRepository ClientInfo { get; }

        Task SaveAsync();
    }
}
