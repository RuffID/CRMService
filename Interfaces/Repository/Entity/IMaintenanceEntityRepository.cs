using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IMaintenanceEntityRepository : IGetRepository<MaintenanceEntity>, IUpdateRepository<MaintenanceEntity>, ICreateRepository<MaintenanceEntity>
    {
        Task<MaintenanceEntity?> GetMaintenanceEntityById(int id, bool? trackable = null);
        public Task CreateOrUpdate(IEnumerable<MaintenanceEntity> items);
    }
}