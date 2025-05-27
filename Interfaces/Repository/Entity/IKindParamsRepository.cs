using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindParamsRepository : IGetRepository<KindParam>, IUpdateRepository<KindParam>, ICreateRepository<KindParam>, IDeleteRepository<KindParam>
    {
        Task<IEnumerable<KindParam>?> GetConnectionsByKind(int id, bool? trackable = null);
        Task<KindParam?> GetConnectionByKindId(int id, bool? trackable = null);
    }
}