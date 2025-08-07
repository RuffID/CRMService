using CRMService.Dto.Entity;
using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEquipmentRepository : IGetRepository<Equipment>, IUpdateRepository<Equipment>, ICreateRepository<Equipment>
    {
        Task<IEnumerable<EquipmentDto>?> GetEquipmentsByMaintenanceEntity(int maintenanceId);
        Task<IEnumerable<EquipmentDto>?> GetEquipmentsByCompany(int companyid);
        Task<EquipmentDto?> GetEquipmentById(int equipmentId);
        Task CreateOrUpdate(IEnumerable<Equipment> items);
    }
}