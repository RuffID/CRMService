using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IManufacturerRepository : IGetRepository<Manufacturer>, IUpdateRepository<Manufacturer>, ICreateRepository<Manufacturer>
    {
        public Task CreateOrUpdate(IEnumerable<Manufacturer> items);
    }
}