using CRMService.Dto.Entity;
using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IParameterRepository : IGetRepository<Parameter>, IUpdateRepository<Parameter>, ICreateRepository<Parameter>
    {
        Task<IEnumerable<EquipmentParameterDto>?> GetParameterByEquipmentId(int equipmentId);
        Task<Parameter?> GetParameterByEquipmentAndKindParameterId(Parameter parameter, bool? trackable = null);
        Task CreateOrUpdate(IEnumerable<Parameter> items);
    }
}
