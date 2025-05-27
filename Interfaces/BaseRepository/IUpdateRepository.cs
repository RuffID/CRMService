namespace CRMService.Interfaces.BaseRepository
{
    public interface IUpdateRepository<T> where T : class
    {
        void Update(T item); // обновление объекта
    }
}
