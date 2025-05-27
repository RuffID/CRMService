using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IModelRepository : IGetRepository<Model>, IUpdateRepository<Model>, ICreateRepository<Model>
    {
        public Task CreateOrUpdate(IEnumerable<Model> items);
    }
}